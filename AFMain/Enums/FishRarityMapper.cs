using System;
using System.Collections.Generic;
using Terraria.GameContent.FishDropRules;

namespace AutoFish.AFMain.Enums;

/// <summary>
///     鱼类稀有度枚举与游戏内 FishRarityCondition 对象之间的映射器。
/// </summary>
public static class FishRarityMapper
{
    private static readonly Dictionary<FishRarityType, Func<FishRarityCondition>> RarityMap = new()
    {
        { FishRarityType.Any, () => AFishDropRulePopulator.Rarity.Any },
        { FishRarityType.Legendary, () => AFishDropRulePopulator.Rarity.Legendary },
        { FishRarityType.VeryRare, () => AFishDropRulePopulator.Rarity.VeryRare },
        { FishRarityType.Rare, () => AFishDropRulePopulator.Rarity.Rare },
        { FishRarityType.Uncommon, () => AFishDropRulePopulator.Rarity.Uncommon },
        { FishRarityType.Common, () => AFishDropRulePopulator.Rarity.Common },
        { 
            FishRarityType.BombRarityOfNotLegendaryAndNotVeryRareAndUncommon, 
            () => AFishDropRulePopulator.Rarity.BombRarityOfNotLegendaryAndNotVeryRareAndUncommon 
        },
        { FishRarityType.UncommonOrCommon, () => AFishDropRulePopulator.Rarity.UncommonOrCommon }
    };

    private static readonly Dictionary<string, FishRarityType> StringMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Any", FishRarityType.Any },
        { "Legendary", FishRarityType.Legendary },
        { "VeryRare", FishRarityType.VeryRare },
        { "Rare", FishRarityType.Rare },
        { "Uncommon", FishRarityType.Uncommon },
        { "Common", FishRarityType.Common },
        { "BombRarity", FishRarityType.BombRarityOfNotLegendaryAndNotVeryRareAndUncommon },
        { "UncommonOrCommon", FishRarityType.UncommonOrCommon }
    };

    /// <summary>
    ///     从枚举获取对应的 FishRarityCondition 对象。
    /// </summary>
    public static FishRarityCondition GetRarityCondition(FishRarityType rarityType)
    {
        if (RarityMap.TryGetValue(rarityType, out var factory))
        {
            return factory();
        }
        return AFishDropRulePopulator.Rarity.Common; // 默认返回普通
    }

    /// <summary>
    ///     从字符串解析稀有度枚举。
    /// </summary>
    public static bool TryParse(string rarityString, out FishRarityType rarityType)
    {
        return StringMap.TryGetValue(rarityString, out rarityType);
    }

    /// <summary>
    ///     从字符串获取对应的 FishRarityCondition 对象。
    /// </summary>
    public static FishRarityCondition GetRarityConditionFromString(string rarityString)
    {
        if (TryParse(rarityString, out var rarityType))
        {
            return GetRarityCondition(rarityType);
        }
        return AFishDropRulePopulator.Rarity.Common; // 默认返回普通
    }

    /// <summary>
    ///     将枚举转换为字符串（配置文件友好）。
    /// </summary>
    public static string ToString(FishRarityType rarityType)
    {
        return rarityType switch
        {
            FishRarityType.Any => "Any",
            FishRarityType.Legendary => "Legendary",
            FishRarityType.VeryRare => "VeryRare",
            FishRarityType.Rare => "Rare",
            FishRarityType.Uncommon => "Uncommon",
            FishRarityType.Common => "Common",
            FishRarityType.BombRarityOfNotLegendaryAndNotVeryRareAndUncommon => "BombRarity",
            FishRarityType.UncommonOrCommon => "UncommonOrCommon",
            _ => "Common"
        };
    }

    /// <summary>
    ///     获取所有可用的稀有度字符串列表（用于配置文件提示）。
    /// </summary>
    public static IEnumerable<string> GetAvailableRarityNames()
    {
        return StringMap.Keys;
    }
}
