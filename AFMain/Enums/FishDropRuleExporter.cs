using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.FishDropRules;

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
    public static List<ExportedFishDropRule> ExportRules(bool skipPartiallyMapped = true)
    {
        var ruleList = FishingConditionMapper.RuleList;
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
        var allRules = ExportRules(skipPartiallyMapped: false);
        
        return new ExportStatistics
        {
            TotalRules = allRules.Count,
            FullyMappedRules = allRules.Count(r => r.IsFullyMapped),
            PartiallyMappedRules = allRules.Count(r => !r.IsFullyMapped && r.Conditions.Count > 0),
            UnmappedRules = allRules.Count(r => r.Conditions.Count == 0 && r.UnmappedConditionsCount > 0)
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
