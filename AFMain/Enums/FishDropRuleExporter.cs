using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria.GameContent.FishDropRules;
using TShockAPI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AutoFish.AFMain.Enums;

/// <summary>
///     钓鱼掉落规则数据传输对象，用于导出规则。
/// </summary>
public class ExportedFishDropRule
{
    /// <summary>可能掉落的物品 ID 列表</summary>
    public int[] PossibleItems { get; set; } = [];

    /// <summary>概率分子</summary>
    public int ChanceNumerator { get; set; }

    /// <summary>概率分母</summary>
    public int ChanceDenominator { get; set; }

    /// <summary>稀有度类型</summary>
    public FishRarityType? Rarity { get; set; }

    /// <summary>钓鱼条件类型列表</summary>
    public List<FishingConditionType> Conditions { get; set; } = new();

    /// <summary>无法映射的条件数量</summary>
    public int UnmappedConditionsCount { get; set; }

    /// <summary>是否完全匹配（所有条件都能映射）</summary>
    public bool IsFullyMapped => UnmappedConditionsCount == 0;
}

/// <summary>
///     钓鱼掉落规则导出器，用于从游戏规则反向导出配置。
/// </summary>
public static class FishDropRuleExporter
{
    /// <summary>
    ///     从 FishDropRuleList 导出所有规则，跳过无法完全映射的规则。
    /// </summary>
    /// <param name="skipPartiallyMapped">是否跳过部分映射的规则（默认 true）</param>
    public static List<ExportedFishDropRule> ExportSystemRules(bool skipPartiallyMapped = true)
    {
        var ruleList = FishingConditionMapper.SystemRuleList;
        return ExportRules(ruleList, skipPartiallyMapped);
    }

    /// <summary>
    ///     从指定的 FishDropRuleList 导出规则。
    /// </summary>
    public static List<ExportedFishDropRule> ExportRules(FishDropRuleList ruleList, bool skipPartiallyMapped = true)
    {
        var exportedRules = new List<ExportedFishDropRule>();
        
        // 使用反射获取 FishDropRuleList 中的规则列表
        var rules = ruleList._rules;
        
        foreach (var rule in rules)
        {
            var exported = ExportRule(rule);
            
            // 根据选项决定是否跳过部分映射的规则
            if (skipPartiallyMapped && !exported.IsFullyMapped)
            {
                continue;
            }
            
            exportedRules.Add(exported);
        }
        
        return exportedRules;
    }

    /// <summary>
    ///     导出单个规则。
    /// </summary>
    public static ExportedFishDropRule ExportRule(FishDropRule rule)
    {
        var exported = new ExportedFishDropRule
        {
            PossibleItems = rule.PossibleItems ?? [],
            ChanceNumerator = rule.ChanceNumerator,
            ChanceDenominator = rule.ChanceDenominator
        };

        // 映射稀有度
        if (rule.Rarity != null && FishRarityMapper.TryGetRarityType(rule.Rarity, out var rarityType))
        {
            exported.Rarity = rarityType;
        }

        // 映射钓鱼条件
        if (rule.Conditions != null && rule.Conditions.Length > 0)
        {
            var mappedConditions = FishingConditionMapper.GetConditionTypes(rule.Conditions);
            exported.Conditions = mappedConditions;
            exported.UnmappedConditionsCount = rule.Conditions.Length - mappedConditions.Count;
        }

        return exported;
    }

    /// <summary>
    ///     获取所有规则的统计信息。
    /// </summary>
    public static ExportStatistics GetStatistics()
    {
        var allRules = ExportSystemRules(skipPartiallyMapped: false);
        
        return new ExportStatistics
        {
            TotalRules = allRules.Count,
            FullyMappedRules = allRules.Count(r => r.IsFullyMapped),
            PartiallyMappedRules = allRules.Count(r => !r.IsFullyMapped && r.Conditions.Count > 0),
            UnmappedRules = allRules.Count(r => r.Conditions.Count == 0 && r.UnmappedConditionsCount > 0)
        };
    }

    /// <summary>
    ///     导出规则到 YAML 文件。
    /// </summary>
    public static void ExportToYamlFile(string filePath, bool skipPartiallyMapped = true)
    {
        var rules = ExportSystemRules(skipPartiallyMapped);
        var stats = GetStatistics();
        
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithIndentedSequences()
            .Build();

        var sb = new StringBuilder();
        sb.AppendLine("# 钓鱼规则导出文件");
        sb.AppendLine("# 此文件由系统自动生成");
        sb.AppendLine("# 由ksqeib的AutoFishR生成，其中无法匹配规则为任务鱼/特殊世界规则");
        sb.AppendLine();
        sb.AppendLine("rules:");
        
        foreach (var rule in rules)
        {
            sb.AppendLine("  - possibleItems:");
            foreach (var itemId in rule.PossibleItems)
            {
                var itemName = TShock.Utils.GetItemById(itemId)?.Name ?? $"Unknown({itemId})";
                sb.AppendLine($"      - {itemId} # {itemName}");
            }
            sb.AppendLine($"    chanceNumerator: {rule.ChanceNumerator}");
            sb.AppendLine($"    chanceDenominator: {rule.ChanceDenominator}");
            
            if (rule.Rarity.HasValue)
            {
                sb.AppendLine($"    rarity: {FishRarityMapper.ToString(rule.Rarity.Value)}");
            }
            
            if (rule.Conditions.Count > 0)
            {
                sb.AppendLine("    conditions:");
                foreach (var condition in rule.Conditions)
                {
                    sb.AppendLine($"      - {FishingConditionMapper.ToString(condition)}");
                }
            }
            sb.AppendLine();
        }

        // 添加统计信息
        sb.AppendLine();
        sb.AppendLine("# ====== 导出统计报告 ======");
        sb.AppendLine($"# 总规则数: {stats.TotalRules}");
        sb.AppendLine($"# 完全匹配: {stats.FullyMappedRules} ({GetPercentage(stats.FullyMappedRules, stats.TotalRules)}%)");
        sb.AppendLine($"# 部分匹配: {stats.PartiallyMappedRules} ({GetPercentage(stats.PartiallyMappedRules, stats.TotalRules)}%)");
        sb.AppendLine($"# 无法映射: {stats.UnmappedRules} ({GetPercentage(stats.UnmappedRules, stats.TotalRules)}%)");
        sb.AppendLine($"# 已导出: {rules.Count} 条规则");

        File.WriteAllText(filePath, sb.ToString());
    }

    /// <summary>
    ///     计算百分比。
    /// </summary>
    private static double GetPercentage(int value, int total)
    {
        if (total == 0) return 0;
        return Math.Round(value * 100.0 / total, 2);
    }

    /// <summary>
    ///     需要添加Stopper的特殊条件类型（不包括AnyEnemies，它作为默认头部Stopper）。
    /// </summary>
    private static readonly HashSet<FishingConditionType> StopperConditions = new()
    {
        FishingConditionType.InLava,
        FishingConditionType.InHoney,
        FishingConditionType.Junk,
        FishingConditionType.Crate,
        FishingConditionType.Ocean
    };

    /// <summary>
    ///     从配置文件加载自定义规则到 RuleList。
    ///     会按照特殊区域分组，并在每组后添加 Stopper 以匹配游戏原版逻辑。
    /// </summary>
    public static int LoadCustomRulesToList(List<CustomFishDropRule> customRules, FishDropRuleList targetList)
    {
        int loadedCount = 0;

        // 首先添加 AnyEnemies 的默认头部 Stopper（如果有怪物生成就不进行规则匹配）
        var anyEnemiesCondition = FishingConditionMapper.GetCondition(FishingConditionType.AnyEnemies);
        if (anyEnemiesCondition != null)
        {
            var anyEnemiesStopper = new FishDropRule
            {
                PossibleItems = Array.Empty<int>(),
                ChanceNumerator = 1,
                ChanceDenominator = 1,
                Rarity = AFishDropRulePopulator.Rarity.Any,
                Conditions = new[] { anyEnemiesCondition }
            };
            targetList._rules.Add(anyEnemiesStopper);
            TShock.Log.ConsoleInfo("[AutoFish] 已添加 AnyEnemies 默认头部 Stopper");
        }

        // 使用特殊的键类型来分组（-1表示没有特殊条件）
        var groupedRules = new Dictionary<int, List<(CustomFishDropRule rule, List<AFishingCondition> conditions)>>();
        
        foreach (var customRule in customRules)
        {
            try
            {
                // 解析条件
                var conditions = new List<AFishingCondition>();
                FishingConditionType? stopperCondition = null;
                
                foreach (var conditionStr in customRule.Conditions)
                {
                    var condition = FishingConditionMapper.GetConditionFromString(conditionStr);
                    if (condition != null)
                    {
                        conditions.Add(condition);
                        
                        // 检查是否包含特殊条件
                        if (FishingConditionMapper.TryGetConditionType(condition, out var conditionType))
                        {
                            if (StopperConditions.Contains(conditionType))
                            {
                                // 优先级：AnyEnemies > InLava > InHoney > Junk > Crate > Ocean
                                if (stopperCondition == null || GetStopperPriority(conditionType) < GetStopperPriority(stopperCondition.Value))
                                {
                                    stopperCondition = conditionType;
                                }
                            }
                        }
                    }
                }

                // 按特殊条件分组（没有特殊条件的归为-1组）
                int groupKey = stopperCondition.HasValue ? (int)stopperCondition.Value : -1;
                if (!groupedRules.ContainsKey(groupKey))
                {
                    groupedRules[groupKey] = new List<(CustomFishDropRule, List<AFishingCondition>)>();
                }
                groupedRules[groupKey].Add((customRule, conditions));
            }
            catch (Exception ex)
            {
                TShock.Log.Error($"解析自定义钓鱼规则失败: {ex.Message}");
            }
        }

        // 按优先级顺序处理各组（特殊条件组优先）
        var orderedGroups = groupedRules
            .OrderBy(kvp => kvp.Key == -1 ? int.MaxValue : GetStopperPriority((FishingConditionType)kvp.Key));

        foreach (var group in orderedGroups)
        {
            var groupKey = group.Key;
            var rulesInGroup = group.Value;

            // 加载该组的所有规则
            foreach (var (customRule, conditions) in rulesInGroup)
            {
                try
                {
                    // 解析稀有度
                    FishRarityCondition? rarity = null;
                    if (!string.IsNullOrEmpty(customRule.Rarity))
                    {
                        rarity = FishRarityMapper.GetRarityConditionFromString(customRule.Rarity);
                    }

                    // 创建游戏规则
                    var gameRule = new FishDropRule
                    {
                        PossibleItems = customRule.PossibleItems.ToArray(),
                        ChanceNumerator = customRule.ChanceNumerator,
                        ChanceDenominator = customRule.ChanceDenominator,
                        Rarity = rarity ?? AFishDropRulePopulator.Rarity.Common,
                        Conditions = conditions.ToArray()
                    };

                    targetList._rules.Add(gameRule);
                    loadedCount++;
                }
                catch (Exception ex)
                {
                    TShock.Log.Error($"加载自定义钓鱼规则失败: {ex.Message}");
                }
            }

            // 如果该组有特殊条件，添加 Stopper
            if (groupKey != -1)
            {
                var stopperConditionType = (FishingConditionType)groupKey;
                var stopperConditionInstance = FishingConditionMapper.GetCondition(stopperConditionType);
                if (stopperConditionInstance != null)
                {
                    var stopper = new FishDropRule
                    {
                        PossibleItems = Array.Empty<int>(),  // 空物品列表表示这是一个 Stopper
                        ChanceNumerator = 1,
                        ChanceDenominator = 1,
                        Rarity = AFishDropRulePopulator.Rarity.Any,
                        Conditions = new[] { stopperConditionInstance }
                    };
                    targetList._rules.Add(stopper);
                    
                    TShock.Log.ConsoleInfo($"[AutoFish] 为条件 {stopperConditionType} 添加了 Stopper");
                }
            }
        }

        return loadedCount;
    }

    /// <summary>
    ///     获取特殊条件的优先级（数值越小优先级越高）。
    /// </summary>
    private static int GetStopperPriority(FishingConditionType conditionType)
    {
        return conditionType switch
        {
            FishingConditionType.InLava => 0,
            FishingConditionType.InHoney => 1,
            FishingConditionType.Junk => 2,
            FishingConditionType.Crate => 3,
            FishingConditionType.Ocean => 4,
            _ => int.MaxValue
        };
    }
}

/// <summary>
///     导出统计信息。
/// </summary>
public class ExportStatistics
{
    /// <summary>总规则数</summary>
    public int TotalRules { get; set; }

    /// <summary>完全映射的规则数</summary>
    public int FullyMappedRules { get; set; }

    /// <summary>部分映射的规则数</summary>
    public int PartiallyMappedRules { get; set; }

    /// <summary>无法映射的规则数</summary>
    public int UnmappedRules { get; set; }
}
