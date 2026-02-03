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
    ///     从配置文件加载自定义规则到 RuleList。
    /// </summary>
    public static int LoadCustomRulesToList(List<CustomFishDropRule> customRules, FishDropRuleList targetList)
    {
        int loadedCount = 0;

        foreach (var customRule in customRules)
        {
            try
            {
                // 解析稀有度
                FishRarityCondition? rarity = null;
                if (!string.IsNullOrEmpty(customRule.Rarity))
                {
                    rarity = FishRarityMapper.GetRarityConditionFromString(customRule.Rarity);
                }

                // 解析条件
                var conditions = new List<AFishingCondition>();
                foreach (var conditionStr in customRule.Conditions)
                {
                    var condition = FishingConditionMapper.GetConditionFromString(conditionStr);
                    if (condition != null)
                    {
                        conditions.Add(condition);
                    }
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

        return loadedCount;
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
