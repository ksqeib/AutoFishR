namespace AutoFish.AFMain.Enums;

/// <summary>
///     鱼类稀有度枚举，对应游戏中的 FishRarityCondition。
/// </summary>
public enum FishRarityType
{
    /// <summary>任意稀有度（总是匹配）。</summary>
    Any,

    /// <summary>传奇（Legendary）。</summary>
    Legendary,

    /// <summary>非常稀有（Very Rare）。</summary>
    VeryRare,

    /// <summary>稀有（Rare）。</summary>
    Rare,

    /// <summary>不常见（Uncommon）。</summary>
    Uncommon,

    /// <summary>普通（Common）。</summary>
    Common,

    /// <summary>炸弹稀有度：非传奇且非非常稀有且不常见。</summary>
    BombRarityOfNotLegendaryAndNotVeryRareAndUncommon,

    /// <summary>不常见或普通。</summary>
    UncommonOrCommon
}
