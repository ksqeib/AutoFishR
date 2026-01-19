using System;
using System.Linq;
using System.Text;
using AutoFish.Data;
using TShockAPI;

namespace AutoFish.AFMain;

/// <summary>
///     玩家侧指令处理。
/// </summary>
public partial class Commands
{
    private static void AppendPlayerHelp(TSPlayer player, StringBuilder helpMessage)
    {
        // 个人指令（按权限和全局开关过滤）
        helpMessage.Append("\n/af -- 查看自动钓鱼菜单");
        helpMessage.Append("\n/af status -- 查看个人状态");

        if (AutoFish.Config.GlobalAutoFishFeatureEnabled && AutoFish.HasFeaturePermission(player, "autofish.fish"))
            helpMessage.Append("\n/af fish -- 开启丨关闭[c/4686D4:自动钓鱼]功能");

        if (AutoFish.Config.GlobalBuffFeatureEnabled && AutoFish.HasFeaturePermission(player, "autofish.buff"))
            helpMessage.Append("\n/af buff -- 开启丨关闭[c/F6B152:钓鱼BUFF]");

        if (AutoFish.Config.GlobalMultiHookFeatureEnabled && AutoFish.HasFeaturePermission(player, "autofish.multihook"))
        {
            helpMessage.Append("\n/af multi -- 开启丨关闭[c/87DF86:多钩功能]");
            helpMessage.Append("\n/af hook 数字 -- 设置个人钩子上限 (<= 全局上限)");
        }

        if (AutoFish.Config.GlobalSkipNonStackableLoot && AutoFish.HasFeaturePermission(player, "autofish.filter.unstackable"))
            helpMessage.Append("\n/af stack -- 开启丨关闭[c/F4C17F:过滤不可堆叠渔获]");

        if (AutoFish.Config.GlobalBlockMonsterCatch && AutoFish.HasFeaturePermission(player, "autofish.filter.monster"))
            helpMessage.Append("\n/af monster -- 开启丨关闭[c/F48FB1:不钓怪物]");

        if (AutoFish.Config.GlobalSkipFishingAnimation && AutoFish.HasFeaturePermission(player, "autofish.skipanimation"))
            helpMessage.Append("\n/af anim -- 开启丨关闭[c/8EC4F4:跳过上鱼动画]");

        if (AutoFish.Config.GlobalConsumptionModeEnabled)
            helpMessage.Append("\n/af list -- 列出消耗模式[c/F5F251:指定物品表]");

        if (AutoFish.Config.ExtraCatchItemIds.Count != 0)
            helpMessage.Append("\n/af loot -- 查看[c/F25055:额外渔获表]");
    }

    private static bool HandlePlayerCommand(CommandArgs args, AFPlayerData.ItemData playerData, double remainingMinutes)
    {
        var player = args.Player;
        var sub = args.Parameters[0].ToLower();

        if (args.Parameters.Count == 1)
        {
            switch (sub)
            {
                case "fish":
                    if (!AutoFish.HasFeaturePermission(player, "autofish.fish"))
                    {
                        args.Player.SendErrorMessage("你没有权限使用自动钓鱼功能。");
                        return true;
                    }

                    var fishEnabled = playerData.AutoFishEnabled;
                    playerData.AutoFishEnabled = !fishEnabled;
                    args.Player.SendSuccessMessage($"玩家 [{args.Player.Name}] 已[c/92C5EC:{(fishEnabled ? "禁用" : "启用")}]自动钓鱼功能。");
                    return true;
                case "buff":
                    if (!AutoFish.HasFeaturePermission(player, "autofish.buff"))
                    {
                        args.Player.SendErrorMessage("你没有权限使用自动钓鱼BUFF功能。");
                        return true;
                    }

                    var isEnabled = playerData.BuffEnabled;
                    playerData.BuffEnabled = !isEnabled;
                    args.Player.SendSuccessMessage($"玩家 [{args.Player.Name}] 已[c/92C5EC:{(isEnabled ? "禁用" : "启用")}]自动钓鱼BUFF");
                    return true;
                case "multi":
                    if (!AutoFish.HasFeaturePermission(player, "autofish.multihook"))
                    {
                        args.Player.SendErrorMessage("你没有权限使用多钩功能。");
                        return true;
                    }

                    if (!AutoFish.Config.GlobalMultiHookFeatureEnabled)
                    {
                        args.Player.SendWarningMessage("多钩功能未在全局开启，无法切换。");
                        return true;
                    }

                    playerData.MultiHookEnabled = !playerData.MultiHookEnabled;
                    args.Player.SendSuccessMessage($"玩家 [{args.Player.Name}] 已[c/92C5EC:{(playerData.MultiHookEnabled ? "启用" : "禁用")}]多钩功能。");
                    return true;
                case "status":
                    SendStatus(args.Player, playerData, remainingMinutes);
                    return true;
                case "stack":
                    if (!AutoFish.Config.GlobalSkipNonStackableLoot)
                    {
                        args.Player.SendWarningMessage("过滤不可堆叠未在全局开启，无法切换。");
                        return true;
                    }

                    if (!AutoFish.HasFeaturePermission(player, "autofish.filter.unstackable"))
                    {
                        args.Player.SendErrorMessage("你没有权限使用过滤不可堆叠功能。");
                        return true;
                    }

                    playerData.SkipNonStackableLoot = !playerData.SkipNonStackableLoot;
                    args.Player.SendSuccessMessage($"玩家 [{args.Player.Name}] 已[c/92C5EC:{(playerData.SkipNonStackableLoot ? "启用" : "禁用")}]过滤不可堆叠渔获。");
                    return true;
                case "monster":
                    if (!AutoFish.Config.GlobalBlockMonsterCatch)
                    {
                        args.Player.SendWarningMessage("不钓怪物未在全局开启，无法切换。");
                        return true;
                    }

                    if (!AutoFish.HasFeaturePermission(player, "autofish.filter.monster"))
                    {
                        args.Player.SendErrorMessage("你没有权限使用不钓怪物功能。");
                        return true;
                    }

                    playerData.BlockMonsterCatch = !playerData.BlockMonsterCatch;
                    args.Player.SendSuccessMessage($"玩家 [{args.Player.Name}] 已[c/92C5EC:{(playerData.BlockMonsterCatch ? "启用" : "禁用")}]不钓怪物。");
                    return true;
                case "anim":
                    if (!AutoFish.Config.GlobalSkipFishingAnimation)
                    {
                        args.Player.SendWarningMessage("跳过上鱼动画未在全局开启，无法切换。");
                        return true;
                    }

                    if (!AutoFish.HasFeaturePermission(player, "autofish.skipanimation"))
                    {
                        args.Player.SendErrorMessage("你没有权限使用跳过上鱼动画功能。");
                        return true;
                    }

                    playerData.SkipFishingAnimation = !playerData.SkipFishingAnimation;
                    args.Player.SendSuccessMessage($"玩家 [{args.Player.Name}] 已[c/92C5EC:{(playerData.SkipFishingAnimation ? "启用" : "禁用")}]跳过上鱼动画。");
                    return true;
                case "list" when AutoFish.Config.BaitItemIds.Any():
                    args.Player.SendInfoMessage("[指定消耗物品表]\n" + string.Join(", ",
                        AutoFish.Config.BaitItemIds.Select(x =>
                            TShock.Utils.GetItemById(x).Name + "([c/92C5EC:{0}])".SFormat(x))));
                    args.Player.SendSuccessMessage(
                        $"兑换规则为：每[c/F5F252:{AutoFish.Config.BaitConsumeCount}]个 => [c/92C5EC:{AutoFish.Config.RewardDurationMinutes}]分钟");
                    return true;
                case "loot" when AutoFish.Config.ExtraCatchItemIds.Any():
                    args.Player.SendInfoMessage("[额外渔获表]\n" + string.Join(", ",
                        AutoFish.Config.ExtraCatchItemIds.Select(x =>
                            TShock.Utils.GetItemById(x).Name + "([c/92C5EC:{0}])".SFormat(x))));
                    return true;
                default:
                    return false;
            }
        }

        if (args.Parameters.Count == 2)
        {
            switch (sub)
            {
                case "hook":
                    if (!AutoFish.HasFeaturePermission(args.Player, "autofish.multihook"))
                    {
                        args.Player.SendErrorMessage("你没有权限设置钩子上限。");
                        return true;
                    }

                    if (!AutoFish.Config.GlobalMultiHookFeatureEnabled)
                    {
                        args.Player.SendWarningMessage("多钩功能未在全局开启，无法设置钩子数量。");
                        return true;
                    }

                    if (!int.TryParse(args.Parameters[1], out var personalMax))
                    {
                        args.Player.SendErrorMessage("请输入数字，格式：/af hook 数字");
                        return true;
                    }

                    if (personalMax < 1)
                    {
                        args.Player.SendWarningMessage("钩子数量必须大于等于 1。");
                        return true;
                    }

                    if (personalMax > AutoFish.Config.GlobalMultiHookMaxNum)
                    {
                        args.Player.SendWarningMessage($"已限制为全局上限：{AutoFish.Config.GlobalMultiHookMaxNum}。");
                        personalMax = AutoFish.Config.GlobalMultiHookMaxNum;
                    }

                    playerData.HookMaxNum = personalMax;
                    args.Player.SendSuccessMessage($"已将个人钩子上限设置为：{personalMax} (全局上限 {AutoFish.Config.GlobalMultiHookMaxNum})");
                    return true;
                default:
                    return false;
            }
        }

        return false;
    }
}