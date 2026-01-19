using System.Collections.Generic;
using Terraria;
using TShockAPI;

namespace AutoFish.Utils;

public class Tools
{
    /// <summary>
    ///     检查指定玩家是否有任意活跃的浮漂。
    /// </summary>
    public static bool BobbersActive(int whoAmI)
    {
        return Main.projectile.Any(p => p.active && p.owner == whoAmI && p.bobber);
    }

    /// <summary>
    ///     将当前使用的贵重鱼饵与背包中最末尾的可用鱼饵交换，以避免消耗贵重鱼饵。
    /// </summary>
    /// <returns>是否发生交换。</returns>
    public static bool TrySwapValuableBaitToBack(TSPlayer player, int baitType, ICollection<int> valuableBaitIds,
        out int currentSlot, out int targetSlot)
    {
        currentSlot = -1;
        targetSlot = -1;

        if (player?.TPlayer?.inventory is null) return false;
        var inv = player.TPlayer.inventory;

        for (var i = 0; i < inv.Length; i++)
        {
            if (inv[i].bait <= 0 || inv[i].type != baitType) continue;
            currentSlot = i;
            break;
        }

        if (currentSlot == -1) return false;

        // 优先选择末尾的非贵重鱼饵，找不到时才回退到任何末尾鱼饵。
        for (var i = inv.Length - 1; i >= 0; i--)
        {
            if (inv[i].bait <= 0) continue;
            if (valuableBaitIds.Contains(inv[i].type)) continue;
            targetSlot = i;
            break;
        }

        if (targetSlot == -1)
        {
            for (var i = inv.Length - 1; i >= 0; i--)
            {
                if (inv[i].bait <= 0 || i == currentSlot) continue;
                targetSlot = i;
                break;
            }
        }

        if (targetSlot == -1 || targetSlot == currentSlot) return false;

        (inv[currentSlot], inv[targetSlot]) = (inv[targetSlot], inv[currentSlot]);
        return true;
    }

    /// <summary>
    ///     根据在线玩家数量动态调整钓鱼次数上限。
    /// </summary>
    public static int GetLimit(int plrs)
    {
        return plrs <= 5 ? 100 : plrs <= 10 ? 50 : plrs <= 20 ? 25 : 10;
    }
}