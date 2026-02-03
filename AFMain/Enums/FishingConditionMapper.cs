using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.FishDropRules;

namespace AutoFish.AFMain.Enums;

/// <summary>
///     钓鱼条件枚举与游戏内 AFishingCondition 对象之间的映射器。
/// </summary>
public static class FishingConditionMapper
{
    /// <summary>
    ///     静态的游戏内容钓鱼掉落规则填充器实例，用于访问所有钓鱼条件。
    /// </summary>
    public static readonly FishDropRuleList SystemRuleList = new();
    public static readonly GameContentFishDropPopulator Populator = new(SystemRuleList);

    /// <summary>
    ///     静态构造函数，在类第一次被访问时自动调用一次，用于初始化钓鱼掉落规则。
    /// </summary>
    static FishingConditionMapper()
    {
        // 调用 Populate() 方法填充钓鱼掉落规则列表
        Populator.Populate();
        
        // 构建反向映射字典（从 AFishingCondition 对象到枚举类型）
        BuildReverseMap();
    }

    // 反向映射字典，用于从 AFishingCondition 对象查找对应的枚举
    private static readonly Dictionary<AFishingCondition, FishingConditionType> ReverseConditionMap = new();

    /// <summary>
    ///     构建反向映射字典。
    /// </summary>
    private static void BuildReverseMap()
    {
        foreach (var kvp in ConditionMap)
        {
            var condition = kvp.Value();
            if (!ReverseConditionMap.ContainsKey(condition))
            {
                ReverseConditionMap[condition] = kvp.Key;
            }
        }
    }

    private static readonly Dictionary<FishingConditionType, Func<AFishingCondition>> ConditionMap = new()
    {
        { FishingConditionType.HardMode, () => Populator.HardMode },
        { FishingConditionType.EarlyMode, () => Populator.EarlyMode },
        { FishingConditionType.InLava, () => Populator.InLava },
        { FishingConditionType.InHoney, () => Populator.InHoney },
        { FishingConditionType.Junk, () => Populator.Junk },
        { FishingConditionType.Crate, () => Populator.Crate },
        { FishingConditionType.AnyEnemies, () => Populator.AnyEnemies },
        { FishingConditionType.CanFishInLava, () => Populator.CanFishInLava },
        { FishingConditionType.Dungeon, () => Populator.Dungeon },
        { FishingConditionType.Beach, () => Populator.Beach },
        { FishingConditionType.Hallow, () => Populator.Hallow },
        { FishingConditionType.GlowingMushrooms, () => Populator.GlowingMushrooms },
        { FishingConditionType.TrueDesert, () => Populator.TrueDesert },
        { FishingConditionType.TrueSnow, () => Populator.TrueSnow },
        { FishingConditionType.Remix, () => Populator.Remix },
        { FishingConditionType.Height0, () => Populator.Height0 },
        { FishingConditionType.Height1, () => Populator.Height1 },
        { FishingConditionType.Height2, () => Populator.Height2 },
        { FishingConditionType.Height3, () => Populator.Height3 },
        { FishingConditionType.Height1And2, () => Populator.Height1And2 },
        { FishingConditionType.HeightAbove1, () => Populator.HeightAbove1 },
        { FishingConditionType.HeightAboveAnd1, () => Populator.HeightAboveAnd1 },
        { FishingConditionType.HeightUnder2, () => Populator.HeightUnder2 },
        { FishingConditionType.HeightAbove2, () => Populator.HeightAbove2 },
        { FishingConditionType.UnderRockLayer, () => Populator.UnderRockLayer },
        { FishingConditionType.Corruption, () => Populator.Corruption },
        { FishingConditionType.Crimson, () => Populator.Crimson },
        { FishingConditionType.Jungle, () => Populator.Jungle },
        { FishingConditionType.Snow, () => Populator.Snow },
        { FishingConditionType.Desert, () => Populator.Desert },
        { FishingConditionType.RolledHallowDesert, () => Populator.RolledHallowDesert },
        { FishingConditionType.OriginalOcean, () => Populator.OriginalOcean },
        { FishingConditionType.RemixOcean, () => Populator.RemixOcean },
        { FishingConditionType.Ocean, () => Populator.Ocean },
        { FishingConditionType.Water1000, () => Populator.Water1000 },
        { FishingConditionType.BloodMoon, () => Populator.BloodMoon },
        { FishingConditionType.DidNotUseCombatBook, () => Populator.DidNotUseCombatBook }
    };

    private static readonly Dictionary<string, FishingConditionType> StringMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "HardMode", FishingConditionType.HardMode },
        { "EarlyMode", FishingConditionType.EarlyMode },
        { "InLava", FishingConditionType.InLava },
        { "InHoney", FishingConditionType.InHoney },
        { "Junk", FishingConditionType.Junk },
        { "Crate", FishingConditionType.Crate },
        { "AnyEnemies", FishingConditionType.AnyEnemies },
        { "CanFishInLava", FishingConditionType.CanFishInLava },
        { "Dungeon", FishingConditionType.Dungeon },
        { "Beach", FishingConditionType.Beach },
        { "Hallow", FishingConditionType.Hallow },
        { "GlowingMushrooms", FishingConditionType.GlowingMushrooms },
        { "TrueDesert", FishingConditionType.TrueDesert },
        { "TrueSnow", FishingConditionType.TrueSnow },
        { "Remix", FishingConditionType.Remix },
        { "Height0", FishingConditionType.Height0 },
        { "Height1", FishingConditionType.Height1 },
        { "Height2", FishingConditionType.Height2 },
        { "Height3", FishingConditionType.Height3 },
        { "Height1And2", FishingConditionType.Height1And2 },
        { "HeightAbove1", FishingConditionType.HeightAbove1 },
        { "HeightAboveAnd1", FishingConditionType.HeightAboveAnd1 },
        { "HeightUnder2", FishingConditionType.HeightUnder2 },
        { "HeightAbove2", FishingConditionType.HeightAbove2 },
        { "UnderRockLayer", FishingConditionType.UnderRockLayer },
        { "Corruption", FishingConditionType.Corruption },
        { "Crimson", FishingConditionType.Crimson },
        { "Jungle", FishingConditionType.Jungle },
        { "Snow", FishingConditionType.Snow },
        { "Desert", FishingConditionType.Desert },
        { "RolledHallowDesert", FishingConditionType.RolledHallowDesert },
        { "OriginalOcean", FishingConditionType.OriginalOcean },
        { "RemixOcean", FishingConditionType.RemixOcean },
        { "Ocean", FishingConditionType.Ocean },
        { "Water1000", FishingConditionType.Water1000 },
        { "BloodMoon", FishingConditionType.BloodMoon },
        { "DidNotUseCombatBook", FishingConditionType.DidNotUseCombatBook }
    };

    /// <summary>
    ///     从枚举获取对应的 AFishingCondition 对象。
    /// </summary>
    public static AFishingCondition GetCondition(FishingConditionType conditionType)
    {
        if (ConditionMap.TryGetValue(conditionType, out var factory))
        {
            return factory();
        }
        throw new ArgumentException($"未找到钓鱼条件类型: {conditionType}", nameof(conditionType));
    }

    /// <summary>
    ///     从字符串解析钓鱼条件枚举。
    /// </summary>
    public static bool TryParse(string conditionString, out FishingConditionType conditionType)
    {
        return StringMap.TryGetValue(conditionString, out conditionType);
    }

    /// <summary>
    ///     从字符串获取对应的 AFishingCondition 对象。
    /// </summary>
    public static AFishingCondition? GetConditionFromString(string conditionString)
    {
        if (TryParse(conditionString, out var conditionType))
        {
            return GetCondition(conditionType);
        }
        return null;
    }

    /// <summary>
    ///     将枚举转换为字符串（配置文件友好）。
    /// </summary>
    public static string ToString(FishingConditionType conditionType)
    {
        return conditionType switch
        {
            FishingConditionType.HardMode => "HardMode",
            FishingConditionType.EarlyMode => "EarlyMode",
            FishingConditionType.InLava => "InLava",
            FishingConditionType.InHoney => "InHoney",
            FishingConditionType.Junk => "Junk",
            FishingConditionType.Crate => "Crate",
            FishingConditionType.AnyEnemies => "AnyEnemies",
            FishingConditionType.CanFishInLava => "CanFishInLava",
            FishingConditionType.Dungeon => "Dungeon",
            FishingConditionType.Beach => "Beach",
            FishingConditionType.Hallow => "Hallow",
            FishingConditionType.GlowingMushrooms => "GlowingMushrooms",
            FishingConditionType.TrueDesert => "TrueDesert",
            FishingConditionType.TrueSnow => "TrueSnow",
            FishingConditionType.Remix => "Remix",
            FishingConditionType.Height0 => "Height0",
            FishingConditionType.Height1 => "Height1",
            FishingConditionType.Height2 => "Height2",
            FishingConditionType.Height3 => "Height3",
            FishingConditionType.Height1And2 => "Height1And2",
            FishingConditionType.HeightAbove1 => "HeightAbove1",
            FishingConditionType.HeightAboveAnd1 => "HeightAboveAnd1",
            FishingConditionType.HeightUnder2 => "HeightUnder2",
            FishingConditionType.HeightAbove2 => "HeightAbove2",
            FishingConditionType.UnderRockLayer => "UnderRockLayer",
            FishingConditionType.Corruption => "Corruption",
            FishingConditionType.Crimson => "Crimson",
            FishingConditionType.Jungle => "Jungle",
            FishingConditionType.Snow => "Snow",
            FishingConditionType.Desert => "Desert",
            FishingConditionType.RolledHallowDesert => "RolledHallowDesert",
            FishingConditionType.OriginalOcean => "OriginalOcean",
            FishingConditionType.RemixOcean => "RemixOcean",
            FishingConditionType.Ocean => "Ocean",
            FishingConditionType.Water1000 => "Water1000",
            FishingConditionType.BloodMoon => "BloodMoon",
            FishingConditionType.DidNotUseCombatBook => "DidNotUseCombatBook",
            _ => throw new ArgumentException($"未知的钓鱼条件类型: {conditionType}", nameof(conditionType))
        };
    }

    /// <summary>
    ///     获取所有可用的钓鱼条件字符串列表（用于配置文件提示）。
    /// </summary>
    public static IEnumerable<string> GetAvailableConditionNames()
    {
        return StringMap.Keys;
    }

    /// <summary>
    ///     获取多个钓鱼条件对象。
    /// </summary>
    public static AFishingCondition[] GetConditions(params FishingConditionType[] conditionTypes)
    {
        var conditions = new AFishingCondition[conditionTypes.Length];
        for (int i = 0; i < conditionTypes.Length; i++)
        {
            conditions[i] = GetCondition(conditionTypes[i]);
        }
        return conditions;
    }

    /// <summary>
    ///     反向映射：从 AFishingCondition 对象获取对应的枚举类型。
    /// </summary>
    /// <param name="condition">钓鱼条件对象</param>
    /// <param name="conditionType">输出的枚举类型</param>
    /// <returns>如果找到映射返回 true，否则返回 false</returns>
    public static bool TryGetConditionType(AFishingCondition condition, out FishingConditionType conditionType)
    {
        return ReverseConditionMap.TryGetValue(condition, out conditionType);
    }

    /// <summary>
    ///     反向映射：从多个 AFishingCondition 对象获取对应的枚举类型列表，跳过无法映射的条件。
    /// </summary>
    public static List<FishingConditionType> GetConditionTypes(AFishingCondition[] conditions)
    {
        var result = new List<FishingConditionType>();
        if (conditions == null) return result;

        foreach (var condition in conditions)
        {
            if (TryGetConditionType(condition, out var type))
            {
                result.Add(type);
            }
        }
        return result;
    }

    /// <summary>
    ///     获取钓鱼条件的本地化显示名称。
    /// </summary>
    public static string GetLocalizedName(FishingConditionType conditionType)
    {
        var key = $"condition.{conditionType}";
        return Lang.T(key);
    }
}
