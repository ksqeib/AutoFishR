using System;
using System.Linq;
using System.Text;
using AutoFish.Data;
using Terraria;
using TShockAPI;

namespace AutoFish.AFMain;

/// <summary>
///     自动钓鱼插件的聊天命令处理入口与通用逻辑。
/// </summary>
public partial class Commands
{
    /// <summary>
    ///     处理 /af 相关指令的入口。
    /// </summary>
    public static void Afs(CommandArgs args)
    {
        var player = args.Player;

        if (!AutoFish.Config.PluginEnabled) return;

        var playerData = AutoFish.PlayerData.GetOrCreatePlayerData(player.Name, AutoFish.CreateDefaultPlayerData);

        //消耗模式下的剩余时间记录
        var remainingMinutes = AutoFish.Config.RewardDurationMinutes - (DateTime.Now - playerData.LogTime).TotalMinutes;

        if (args.Parameters.Count == 0)
        {
            HelpCmd(args.Player);

            if (!playerData.AutoFishEnabled)
                args.Player.SendSuccessMessage("请输入该指令开启→: [c/92C5EC:/af fish]");

            //开启了消耗模式
            else if (playerData.ConsumptionEnabled)
                args.Player.SendMessage($"自动钓鱼[c/46C4D4:剩余时长]：[c/F3F292:{Math.Floor(remainingMinutes)}]分钟", 243, 181,
                    145);
            return;
        }

        if (HandlePlayerCommand(args, playerData, remainingMinutes))
            return;

        if (HandleAdminCommand(args))
            return;

        HelpCmd(args.Player);
    }

    /// <summary>
    ///     展示个人状态信息。
    /// </summary>
    private static void SendStatus(TSPlayer player, AFPlayerData.ItemData playerData, double remainingMinutes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[自动钓鱼个人状态]");
        sb.AppendLine($"功能：{(playerData.AutoFishEnabled ? "开启" : "关闭")}");
        sb.AppendLine($"BUFF：{(playerData.BuffEnabled ? "开启" : "关闭")}");
        sb.AppendLine($"多钩：{(playerData.MultiHookEnabled ? "开启" : "关闭")}, 钩子上限：{playerData.HookMaxNum}");
        sb.AppendLine($"过滤不可堆叠：{(playerData.SkipNonStackableLoot ? "开启" : "关闭")}");
        sb.AppendLine($"不钓怪物：{(playerData.BlockMonsterCatch ? "开启" : "关闭")}");
        sb.AppendLine($"跳过上鱼动画：{(playerData.SkipFishingAnimation ? "开启" : "关闭")}");

        if (AutoFish.Config.BaitItemIds.Any() || playerData.ConsumptionEnabled)
        {
            var minutesLeft = Math.Max(0, Math.Floor(remainingMinutes));
            var consumeLine = playerData.ConsumptionEnabled
                ? $"开启，剩余：{minutesLeft} 分钟"
                : "关闭";
            sb.AppendLine($"消耗模式：{consumeLine}");
        }

        player.SendInfoMessage(sb.ToString());
    }
}