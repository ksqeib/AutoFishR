using System.Text;
using TShockAPI;

namespace AutoFish.AFMain;

/// <summary>
///     管理员侧指令处理。
/// </summary>
public partial class Commands
{
    private static void AppendAdminHelp(TSPlayer player, StringBuilder helpMessage)
    {
        helpMessage.Append("\n[全局] /afa buff -- 开启丨关闭全局钓鱼BUFF");
        helpMessage.Append("\n[全局] /afa multi -- 开启丨关闭多线模式");

        if (AutoFish.Config.GlobalMultiHookFeatureEnabled)
            helpMessage.Append("\n[全局] /afa duo 数字 -- 设置多线的钩子数量上限");

        helpMessage.Append("\n[全局] /afa mod -- 开启丨关闭消耗模式");

        if (AutoFish.Config.GlobalConsumptionModeEnabled)
        {
            helpMessage.Append("\n[全局] /afa set 数量 -- 设置消耗物品数量要求");
            helpMessage.Append("\n[全局] /afa time 数字 -- 设置自动时长(分钟)");
            helpMessage.Append("\n[全局] /afa add 物品名 -- 添加指定鱼饵");
            helpMessage.Append("\n[全局] /afa del 物品名 -- 移除指定鱼饵");
        }

        helpMessage.Append("\n[全局] /afa addloot 物品名 -- 添加额外渔获");
        helpMessage.Append("\n[全局] /afa delloot 物品名 -- 移除额外渔获");
        helpMessage.Append("\n[全局] /afa stack -- 开启丨关闭过滤不可堆叠渔获");
        helpMessage.Append("\n[全局] /afa monster -- 开启丨关闭不钓怪物");
        helpMessage.Append("\n[全局] /afa anim -- 开启丨关闭跳过上鱼动画");
    }

    private static bool HandleAdminCommand(CommandArgs args)
    {
        var caller = args.Player ?? TSPlayer.Server;
        if (!caller.HasPermission(AutoFish.AdminPermission)) return false;

        var sub = args.Parameters[0].ToLower();

        if (args.Parameters.Count == 1)
            switch (sub)
            {
                case "multi":
                    var multiEnabled = AutoFish.Config.GlobalMultiHookFeatureEnabled;
                    AutoFish.Config.GlobalMultiHookFeatureEnabled = !multiEnabled;
                    var multiToggle = multiEnabled ? "禁用" : "启用";
                    caller.SendSuccessMessage($"玩家 [{caller.Name}] 已[c/92C5EC:{multiToggle}]多线模式");
                    AutoFish.Config.Write();
                    return true;
                case "buff":
                    var buffCfgEnabled = AutoFish.Config.GlobalBuffFeatureEnabled;
                    AutoFish.Config.GlobalBuffFeatureEnabled = !buffCfgEnabled;
                    var buffToggleText = buffCfgEnabled ? "禁用" : "启用";
                    caller.SendSuccessMessage($"玩家 [{caller.Name}] 已[c/92C5EC:{buffToggleText}]全局钓鱼BUFF");
                    AutoFish.Config.Write();
                    return true;
                case "mod":
                    var modEnabled = AutoFish.Config.GlobalConsumptionModeEnabled;
                    AutoFish.Config.GlobalConsumptionModeEnabled = !modEnabled;
                    var modToggle = modEnabled ? "禁用" : "启用";
                    caller.SendSuccessMessage($"玩家 [{caller.Name}] 已[c/92C5EC:{modToggle}]消耗模式");
                    AutoFish.Config.Write();
                    return true;
                case "stack":
                    AutoFish.Config.GlobalSkipNonStackableLoot = !AutoFish.Config.GlobalSkipNonStackableLoot;
                    caller.SendSuccessMessage(
                        $"已[c/92C5EC:{(AutoFish.Config.GlobalSkipNonStackableLoot ? "启用" : "禁用")}]全局过滤不可堆叠渔获。");
                    AutoFish.Config.Write();
                    return true;
                case "monster":
                    AutoFish.Config.GlobalBlockMonsterCatch = !AutoFish.Config.GlobalBlockMonsterCatch;
                    caller.SendSuccessMessage(
                        $"已[c/92C5EC:{(AutoFish.Config.GlobalBlockMonsterCatch ? "启用" : "禁用")}]全局不钓怪物。");
                    AutoFish.Config.Write();
                    return true;
                case "anim":
                    AutoFish.Config.GlobalSkipFishingAnimation = !AutoFish.Config.GlobalSkipFishingAnimation;
                    caller.SendSuccessMessage(
                        $"已[c/92C5EC:{(AutoFish.Config.GlobalSkipFishingAnimation ? "启用" : "禁用")}]全局跳过上鱼动画。");
                    AutoFish.Config.Write();
                    return true;
                case "set":
                    caller.SendInfoMessage($"当前消耗物品数量：{AutoFish.Config.BaitConsumeCount}，推荐：2");
                    return true;
                case "time":
                    caller.SendInfoMessage($"当前消耗自动时长：{AutoFish.Config.RewardDurationMinutes}，单位：分钟，推荐：30-45");
                    return true;
                default:
                    return false;
            }

        if (args.Parameters.Count == 2)
        {
            var matchedItems = TShock.Utils.GetItemByIdOrName(args.Parameters[1]);
            if (matchedItems.Count > 1)
            {
                caller.SendMultipleMatchError(matchedItems.Select(i => i.Name));
                return true;
            }

            if (matchedItems.Count == 0)
            {
                caller.SendErrorMessage(
                    "不存在该物品，\"物品查询\": \"[c/92C5EC:https://terraria.wiki.gg/zh/wiki/Item_IDs]\"");
                return true;
            }

            var item = matchedItems[0];

            switch (sub)
            {
                case "add":
                    if (AutoFish.Config.BaitItemIds.Contains(item.type))
                    {
                        caller.SendErrorMessage("物品 [c/92C5EC:{0}] 已在指定鱼饵表中!", item.Name);
                        return true;
                    }

                    AutoFish.Config.BaitItemIds.Add(item.type);
                    AutoFish.Config.Write();
                    caller.SendSuccessMessage("已成功将物品添加指定鱼饵表: [c/92C5EC:{0}]!", item.Name);
                    return true;
                case "del":
                    if (!AutoFish.Config.BaitItemIds.Contains(item.type))
                    {
                        caller.SendErrorMessage("物品 {0} 不在指定鱼饵表中!", item.Name);
                        return true;
                    }

                    AutoFish.Config.BaitItemIds.Remove(item.type);
                    AutoFish.Config.Write();
                    caller.SendSuccessMessage("已成功从指定鱼饵表移出物品: [c/92C5EC:{0}]!", item.Name);
                    return true;
                case "addloot":
                    if (AutoFish.Config.ExtraCatchItemIds.Contains(item.type))
                    {
                        caller.SendErrorMessage("物品 [c/92C5EC:{0}] 已在额外渔获表中!", item.Name);
                        return true;
                    }

                    AutoFish.Config.ExtraCatchItemIds.Add(item.type);
                    AutoFish.Config.Write();
                    caller.SendSuccessMessage("已成功将物品添加额外渔获表: [c/92C5EC:{0}]!", item.Name);
                    return true;
                case "delloot":
                    if (!AutoFish.Config.ExtraCatchItemIds.Contains(item.type))
                    {
                        caller.SendErrorMessage("物品 {0} 不在额外渔获中!", item.Name);
                        return true;
                    }

                    AutoFish.Config.ExtraCatchItemIds.Remove(item.type);
                    AutoFish.Config.Write();
                    caller.SendSuccessMessage("已成功从额外渔获移出物品: [c/92C5EC:{0}]!", item.Name);
                    return true;
                case "set":
                    if (int.TryParse(args.Parameters[1], out var consumeNum))
                    {
                        AutoFish.Config.BaitConsumeCount = consumeNum;
                        AutoFish.Config.Write();
                        caller.SendSuccessMessage("已成功将物品数量要求设置为: [c/92C5EC:{0}] 个!", consumeNum);
                    }

                    return true;
                case "duo":
                    if (int.TryParse(args.Parameters[1], out var maxNum))
                    {
                        AutoFish.Config.GlobalMultiHookMaxNum = maxNum;
                        AutoFish.Config.Write();
                        caller.SendSuccessMessage("已成功将多钩数量上限设置为: [c/92C5EC:{0}] 个!", maxNum);
                    }

                    return true;
                case "time":
                    if (int.TryParse(args.Parameters[1], out var rewardMinutes))
                    {
                        AutoFish.Config.RewardDurationMinutes = rewardMinutes;
                        AutoFish.Config.Write();
                        caller.SendSuccessMessage("已成功将自动时长设置为: [c/92C5EC:{0}] 分钟!", rewardMinutes);
                    }

                    return true;
                default:
                    SendAdminHelpOnly(caller);
                    return true;
            }
        }

        return false;
    }
}