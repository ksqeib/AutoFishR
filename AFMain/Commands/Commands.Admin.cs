using System.Text;
using AutoFish.AFMain.Enums;
using TShockAPI;

namespace AutoFish.AFMain;

/// <summary>
///     管理员侧指令处理�?
/// </summary>
public partial class Commands
{
    private static void AppendAdminHelp(TSPlayer player, StringBuilder helpMessage)
    {
        helpMessage.Append('\n').Append(Lang.T("help.admin.buff"));
        helpMessage.Append('\n').Append(Lang.T("help.admin.multi"));

        if (AutoFish.Config.GlobalMultiHookFeatureEnabled)
            helpMessage.Append('\n').Append(Lang.T("help.admin.duo"));

        helpMessage.Append('\n').Append(Lang.T("help.admin.mod"));

        if (AutoFish.Config.GlobalConsumptionModeEnabled)
        {
            helpMessage.Append('\n').Append(Lang.T("help.admin.set"));
            helpMessage.Append('\n').Append(Lang.T("help.admin.time"));
            helpMessage.Append('\n').Append(Lang.T("help.admin.add"));
            helpMessage.Append('\n').Append(Lang.T("help.admin.del"));
        }

        helpMessage.Append('\n').Append(Lang.T("help.admin.addloot"));
        helpMessage.Append('\n').Append(Lang.T("help.admin.delloot"));
        helpMessage.Append('\n').Append(Lang.T("help.admin.stack"));
        helpMessage.Append('\n').Append(Lang.T("help.admin.monster"));
        helpMessage.Append('\n').Append(Lang.T("help.admin.anim"));
        helpMessage.Append('\n').Append(Lang.T("help.admin.export"));
        helpMessage.Append('\n').Append(Lang.T("help.admin.exportstats"));
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
                    AutoFish.Config.GlobalMultiHookFeatureEnabled = !AutoFish.Config.GlobalMultiHookFeatureEnabled;
                    var multiToggle = Lang.T(AutoFish.Config.GlobalMultiHookFeatureEnabled
                        ? "common.enabledVerb"
                        : "common.disabledVerb");
                    caller.SendSuccessMessage(Lang.T("success.toggle.globalMulti", caller.Name, multiToggle));
                    AutoFish.Config.Write();
                    return true;
                case "buff":
                    AutoFish.Config.GlobalBuffFeatureEnabled = !AutoFish.Config.GlobalBuffFeatureEnabled;
                    var buffToggleText = Lang.T(AutoFish.Config.GlobalBuffFeatureEnabled
                        ? "common.enabledVerb"
                        : "common.disabledVerb");
                    caller.SendSuccessMessage(Lang.T("success.toggle.globalBuff", caller.Name, buffToggleText));
                    AutoFish.Config.Write();
                    return true;
                case "mod":
                    AutoFish.Config.GlobalConsumptionModeEnabled = !AutoFish.Config.GlobalConsumptionModeEnabled;
                    var modToggle = Lang.T(AutoFish.Config.GlobalConsumptionModeEnabled
                        ? "common.enabledVerb"
                        : "common.disabledVerb");
                    caller.SendSuccessMessage(Lang.T("success.toggle.consumption", caller.Name, modToggle));
                    AutoFish.Config.Write();
                    return true;
                case "stack":
                    AutoFish.Config.GlobalSkipNonStackableLoot = !AutoFish.Config.GlobalSkipNonStackableLoot;
                    caller.SendSuccessMessage(Lang.T("success.toggle.globalStack",
                        Lang.T(AutoFish.Config.GlobalSkipNonStackableLoot
                            ? "common.enabledVerb"
                            : "common.disabledVerb")));
                    AutoFish.Config.Write();
                    return true;
                case "monster":
                    AutoFish.Config.GlobalBlockMonsterCatch = !AutoFish.Config.GlobalBlockMonsterCatch;
                    caller.SendSuccessMessage(Lang.T("success.toggle.globalMonster",
                        Lang.T(AutoFish.Config.GlobalBlockMonsterCatch
                            ? "common.enabledVerb"
                            : "common.disabledVerb")));
                    AutoFish.Config.Write();
                    return true;
                case "anim":
                    AutoFish.Config.GlobalSkipFishingAnimation = !AutoFish.Config.GlobalSkipFishingAnimation;
                    caller.SendSuccessMessage(Lang.T("success.toggle.globalAnim",
                        Lang.T(AutoFish.Config.GlobalSkipFishingAnimation
                            ? "common.enabledVerb"
                            : "common.disabledVerb")));
                    AutoFish.Config.Write();
                    return true;
                case "exportstats":
                    ExportStatsCommand(caller);
                    return true;
                default:
                    return false;
            }

        if (args.Parameters.Count == 2)
        {
            // 处理 export 命令
            if (sub == "export")
            {
                var pageStr = args.Parameters[1];
                if (!int.TryParse(pageStr, out var page) || page < 1)
                {
                    caller.SendErrorMessage(Lang.T("error.invalidPage"));
                    return true;
                }
                ExportCommand(caller, page);
                return true;
            }

            var matchedItems = TShock.Utils.GetItemByIdOrName(args.Parameters[1]);
            if (matchedItems.Count > 1)
            {
                caller.SendMultipleMatchError(matchedItems.Select(i => i.Name));
                return true;
            }

            if (matchedItems.Count == 0)
            {
                caller.SendErrorMessage(Lang.T("error.itemNotFound"));
                return true;
            }

            var item = matchedItems[0];

            switch (sub)
            {
                case "del":
                    if (!AutoFish.Config.BaitRewards.ContainsKey(item.type))
                    {
                        caller.SendErrorMessage(Lang.T("error.itemNotInBait", item.Name));
                        return true;
                    }

                    AutoFish.Config.BaitRewards.Remove(item.type);
                    AutoFish.Config.Write();
                    caller.SendSuccessMessage(Lang.T("success.item.removeBait", item.Name));
                    return true;
                case "addloot":
                    if (AutoFish.Config.ExtraCatchItemIds.Contains(item.type))
                    {
                        caller.SendErrorMessage(Lang.T("error.itemAlreadyInLoot", item.Name));
                        return true;
                    }

                    AutoFish.Config.ExtraCatchItemIds.Add(item.type);
                    AutoFish.Config.Write();
                    caller.SendSuccessMessage(Lang.T("success.item.addLoot", item.Name));
                    return true;
                case "delloot":
                    if (!AutoFish.Config.ExtraCatchItemIds.Contains(item.type))
                    {
                        caller.SendErrorMessage(Lang.T("error.itemNotInLoot", item.Name));
                        return true;
                    }

                    AutoFish.Config.ExtraCatchItemIds.Remove(item.type);
                    AutoFish.Config.Write();
                    caller.SendSuccessMessage(Lang.T("success.item.removeLoot", item.Name));
                    return true;
                case "duo":
                    if (int.TryParse(args.Parameters[1], out var maxNum))
                    {
                        AutoFish.Config.GlobalMultiHookMaxNum = maxNum;
                        AutoFish.Config.Write();
                        caller.SendSuccessMessage(Lang.T("success.set.multiMax", maxNum));
                    }

                    return true;
                default:
                    SendAdminHelpOnly(caller);
                    return true;
            }
        }

        // 处理 3 个参数的命令：add, set, time
        if (args.Parameters.Count == 4)
        {
            var matchedItems = TShock.Utils.GetItemByIdOrName(args.Parameters[1]);
            if (matchedItems.Count > 1)
            {
                caller.SendMultipleMatchError(matchedItems.Select(i => i.Name));
                return true;
            }

            if (matchedItems.Count == 0)
            {
                caller.SendErrorMessage(Lang.T("error.itemNotFound"));
                return true;
            }

            var item = matchedItems[0];

            switch (sub)
            {
                case "add":
                case "set":
                    if (!int.TryParse(args.Parameters[2], out var count) || count < 1)
                    {
                        caller.SendErrorMessage("数量必须是大�?0 的整数！");
                        return true;
                    }

                    if (!int.TryParse(args.Parameters[3], out var minutes) || minutes < 1)
                    {
                        caller.SendErrorMessage("分钟数必须是大于 0 的整数！");
                        return true;
                    }

                    AutoFish.Config.BaitRewards[item.type] = new BaitReward
                    {
                        Count = count,
                        Minutes = minutes
                    };
                    AutoFish.Config.Write();
                    caller.SendSuccessMessage(Lang.T("success.set.baitReward", item.Name, count, minutes));
                    return true;
                default:
                    return false;
            }
        }

        // 处理 3 个参数的 time 命令
        if (args.Parameters.Count == 3 && sub == "time")
        {
            var matchedItems = TShock.Utils.GetItemByIdOrName(args.Parameters[1]);
            if (matchedItems.Count > 1)
            {
                caller.SendMultipleMatchError(matchedItems.Select(i => i.Name));
                return true;
            }

            if (matchedItems.Count == 0)
            {
                caller.SendErrorMessage(Lang.T("error.itemNotFound"));
                return true;
            }

            var item = matchedItems[0];

            if (!AutoFish.Config.BaitRewards.ContainsKey(item.type))
            {
                caller.SendErrorMessage(Lang.T("error.itemNotInBait", item.Name));
                return true;
            }

            if (!int.TryParse(args.Parameters[2], out var minutes) || minutes < 1)
            {
                caller.SendErrorMessage("分钟数必须是大于 0 的整数！");
                return true;
            }

            AutoFish.Config.BaitRewards[item.type].Minutes = minutes;
            AutoFish.Config.Write();
            caller.SendSuccessMessage(Lang.T("success.set.baitReward", item.Name,
                AutoFish.Config.BaitRewards[item.type].Count, minutes));
            return true;
        }

        return false;
    }

    /// <summary>
    ///     导出统计信息命令。
    /// </summary>
    private static void ExportStatsCommand(TSPlayer caller)
    {
        try
        {
            var stats = FishDropRuleExporter.GetStatistics();
            
            caller.SendSuccessMessage("=== 钓鱼规则导出统计 ===");
            caller.SendInfoMessage($"总规则数: {stats.TotalRules}");
            caller.SendInfoMessage($"完全匹配: {stats.FullyMappedRules} ({GetPercentage(stats.FullyMappedRules, stats.TotalRules)}%)");
            caller.SendInfoMessage($"部分匹配: {stats.PartiallyMappedRules} ({GetPercentage(stats.PartiallyMappedRules, stats.TotalRules)}%)");
            caller.SendInfoMessage($"无法映射: {stats.UnmappedRules} ({GetPercentage(stats.UnmappedRules, stats.TotalRules)}%)");
            caller.SendInfoMessage($"使用 /afa export <页码> 查看详细规则");
        }
        catch (Exception ex)
        {
            caller.SendErrorMessage($"导出统计失败: {ex.Message}");
            TShock.Log.Error($"导出统计失败: {ex}");
        }
    }

    /// <summary>
    ///     导出规则命令（分页显示）。
    /// </summary>
    private static void ExportCommand(TSPlayer caller, int page)
    {
        try
        {
            var rules = FishDropRuleExporter.ExportRules(skipPartiallyMapped: true);
            
            if (rules.Count == 0)
            {
                caller.SendInfoMessage("没有可导出的完全匹配规则。");
                return;
            }

            const int pageSize = 5;
            var totalPages = (rules.Count + pageSize - 1) / pageSize;
            
            if (page < 1 || page > totalPages)
            {
                caller.SendErrorMessage($"页码超出范围！总共 {totalPages} 页。");
                return;
            }

            var startIndex = (page - 1) * pageSize;
            var endIndex = Math.Min(startIndex + pageSize, rules.Count);
            
            caller.SendSuccessMessage($"=== 钓鱼规则导出 (第 {page}/{totalPages} 页) ===");
            
            for (var i = startIndex; i < endIndex; i++)
            {
                var rule = rules[i];
                var sb = new StringBuilder();
                
                sb.Append($"[{i + 1}] 物品: ");
                if (rule.PossibleItems.Length > 0)
                {
                    var itemNames = rule.PossibleItems
                        .Select(id => TShock.Utils.GetItemById(id)?.Name ?? $"ID:{id}")
                        .Take(3);
                    sb.Append(string.Join(", ", itemNames));
                    if (rule.PossibleItems.Length > 3)
                        sb.Append($" ...共{rule.PossibleItems.Length}个");
                }
                else
                {
                    sb.Append("无");
                }
                
                caller.SendInfoMessage(sb.ToString());
                
                sb.Clear();
                sb.Append($"    概率: {rule.ChanceNumerator}/{rule.ChanceDenominator}");
                if (rule.Rarity.HasValue)
                    sb.Append($", 稀有度: {rule.Rarity.Value}");
                caller.SendInfoMessage(sb.ToString());
                
                if (rule.Conditions.Count > 0)
                {
                    var conditions = string.Join(", ", rule.Conditions.Take(5));
                    if (rule.Conditions.Count > 5)
                        conditions += $" ...共{rule.Conditions.Count}个";
                    caller.SendInfoMessage($"    条件: {conditions}");
                }
            }
            
            if (page < totalPages)
            {
                caller.SendInfoMessage($"使用 /afa export {page + 1} 查看下一页");
            }
        }
        catch (Exception ex)
        {
            caller.SendErrorMessage($"导出规则失败: {ex.Message}");
            TShock.Log.Error($"导出规则失败: {ex}");
        }
    }

    /// <summary>
    ///     计算百分比。
    /// </summary>
    private static double GetPercentage(int value, int total)
    {
        if (total == 0) return 0;
        return Math.Round(value * 100.0 / total, 2);
    }
}

