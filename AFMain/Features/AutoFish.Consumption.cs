using System.Text;
using AutoFish.Data;
using Terraria.ID;
using TShockAPI;

namespace AutoFish.AFMain;

public partial class AutoFish
{
    public static bool CanConsumeFish(TSPlayer player, AFPlayerData.ItemData playerData)
    {
        if (playerData.CanConsume()) return true;
        //没有，尝试续费
        if (ConsumeBaitAndEnableMode(player, playerData))
        {
            return true;
        }

        //续费不了
        ExitTip(player, playerData);
        return false;
    }

    /// <summary>
    ///     消耗鱼饵并启用消耗模式。
    /// </summary>
    private static bool ConsumeBaitAndEnableMode(TSPlayer player, AFPlayerData.ItemData playerData)
    {
        //初始化一个消耗值
        var requiredBait = Config.BaitConsumeCount;

        // 统计背包中指定鱼饵的总数量(不包含手上物品)
        var totalBait = player.TPlayer.inventory.Sum(slot =>
            Config.BaitItemIds.Contains(slot.type) &&
            slot.type != player.TPlayer.inventory[player.TPlayer.selectedItem].type
                ? slot.stack
                : 0);

        // 不够
        if (totalBait < requiredBait) return false;

        // 播报玩家消耗鱼饵用的
        var consumedItemsMessage = new StringBuilder();

        // 遍历背包58格
        for (var i = 0; i < player.TPlayer.inventory.Length && requiredBait > 0; i++)
        {
            var inventorySlot = player.TPlayer.inventory[i];

            // 是Config里指定的鱼饵,不是手上的物品
            if (!Config.BaitItemIds.Contains(inventorySlot.type)) continue;
            var consumedCount = Math.Min(requiredBait, inventorySlot.stack); // 计算需要消耗的鱼饵数量

            inventorySlot.stack -= consumedCount; // 从背包中扣除鱼饵
            requiredBait -= consumedCount; // 减少消耗值

            // 记录消耗的鱼饵数量到播报
            consumedItemsMessage.AppendFormat(" [c/F25156:{0}]([c/AECDD1:{1}]) ",
                TShock.Utils.GetItemById(inventorySlot.type).Name,
                consumedCount);

            // 如果背包中的鱼饵数量为0，清空该格子
            if (inventorySlot.stack < 1) inventorySlot.TurnToAir();

            // 发包给背包里对应格子的鱼饵
            player.SendData(PacketTypes.PlayerSlot, "", player.Index, PlayerItemSlotID.Inventory0 + i);
        }

        // 扣除失败
        if (requiredBait > 0) return false;
        //正常
        playerData.ConsumeOverTime = DateTime.Now.AddMinutes(Config.RewardDurationMinutes);
        player.SendMessage(Lang.T("consumption.enabled", player.Name, consumedItemsMessage.ToString()), 247, 244,
            150);
        if (playerData.ConsumeStartTime == default)
            playerData.ConsumeStartTime = DateTime.Now;
        return true;
    }

    /// <summary>
    ///     消耗模式下检测超时并关闭自动钓鱼权限。
    /// </summary>
    private static void ExitTip(TSPlayer player, AFPlayerData.ItemData playerData)
    {
        //没开启过 不要提示
        if (playerData.ConsumeStartTime == default) return;
        
        var expiredMessage = new StringBuilder();
        expiredMessage.AppendLine(Lang.T("consumption.expired.title"));
        expiredMessage.AppendLine(Lang.T("consumption.expired.body", Config.RewardDurationMinutes));

        // 计算经过的时间（分和秒）
        var timeElapsed = DateTime.Now - playerData.ConsumeStartTime;
        var minutes = (int)timeElapsed.TotalMinutes;
        var seconds = timeElapsed.Seconds;
        playerData.ConsumeStartTime = default;
        
        expiredMessage.AppendFormat(Lang.T("consumption.expired.line"), playerData.Name,
            minutes, seconds);

        player.SendMessage(expiredMessage.ToString(), 247, 244, 150);
    }
}