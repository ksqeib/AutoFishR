using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TShockAPI;

namespace AutoFish.AFMain;

/// <summary>
///     插件配置模型，负责序列化与默认值初始化。
/// </summary>
internal class Configuration
{
    /// <summary>配置文件路径。</summary>
    public static readonly string FilePath = Path.Combine(TShock.SavePath, "AutoFish.json");

    [JsonProperty("插件总开关", Order = 1)] public bool PluginEnabled { get; set; } = true;

    [JsonProperty("全局自动钓鱼开关", Order = 2)] public bool GlobalAutoFishFeatureEnabled { get; set; } = true;
    [JsonProperty("默认自动钓鱼开关", Order = 3)] public bool DefaultAutoFishEnabled { get; set; } = false;

    [JsonProperty("全局Buff开关", Order = 4)] public bool GlobalBuffFeatureEnabled { get; set; } = true;
    [JsonProperty("默认Buff开关", Order = 5)] public bool DefaultBuffEnabled { get; set; } = false;

    [JsonProperty("全局多钩钓鱼开关", Order = 6)] public bool GlobalMultiHookFeatureEnabled { get; set; } = true;
    [JsonProperty("多钩上限", Order = 7)] public int GlobalMultiHookMaxNum { get; set; } = 5;
    [JsonProperty("默认多钩开关", Order = 8)] public bool DefaultMultiHookEnabled { get; set; } = false;

    [JsonProperty("全局消耗模式开关", Order = 9)] public bool GlobalConsumptionModeEnabled { get; set; }
    [JsonProperty("默认消耗模式", Order = 10)] public bool DefaultConsumptionEnabled { get; set; } = false;
    [JsonProperty("消耗数量", Order = 11)] public int BaitConsumeCount { get; set; } = 10;
    [JsonProperty("奖励时长", Order = 12)] public int RewardDurationMinutes { get; set; } = 12;
    [JsonProperty("消耗物品", Order = 13)] public List<int> BaitItemIds { get; set; } = new();

    [JsonProperty("全局过滤不可堆叠物品", Order = 14)] public bool GlobalSkipNonStackableLoot { get; set; } = true;
    [JsonProperty("默认过滤不可堆叠物品", Order = 15)] public bool DefaultSkipNonStackableLoot { get; set; } = true;

    [JsonProperty("全局不钓怪物", Order = 16)] public bool GlobalBlockMonsterCatch { get; set; } = true;
    [JsonProperty("默认不钓怪物", Order = 17)] public bool DefaultBlockMonsterCatch { get; set; } = true;

    [JsonProperty("全局跳过上鱼动画", Order = 18)] public bool GlobalSkipFishingAnimation { get; set; } = true;
    [JsonProperty("默认跳过上鱼动画", Order = 19)] public bool DefaultSkipFishingAnimation { get; set; } = true;

    [JsonProperty("全局保护贵重鱼饵", Order = 20)] public bool GlobalProtectValuableBaitEnabled { get; set; } = true;
    [JsonProperty("默认保护贵重鱼饵", Order = 21)] public bool DefaultProtectValuableBaitEnabled { get; set; } = true;
    [JsonProperty("贵重鱼饵列表", Order = 22)] public List<int> ValuableBaitItemIds { get; set; } = new();

    [JsonProperty("随机物品", Order = 23)] public bool RandomLootEnabled { get; set; }

    [JsonProperty("额外渔获", Order = 24)] public List<int> ExtraCatchItemIds = new();

    [JsonProperty("Buff表", Order = 25)] public Dictionary<int, int> BuffDurations { get; set; } = new();

    [JsonProperty("禁止衍生弹幕", Order = 26)]
    public int[] DisabledProjectileIds { get; set; } =
        new[] { 623, 625, 626, 627, 628, 831, 832, 833, 834, 835, 963, 970 };

    /// <summary>
    ///     初始化默认的 Buff、鱼饵和额外渔获设置。
    /// </summary>
    public void Ints()
    {
        BuffDurations = new Dictionary<int, int>
        {
            // { 80,10 },
            // { 122,240 }
        };

        BaitItemIds = new List<int>
        {
            2002, 2675, 2676, 3191, 3194
        };

        ValuableBaitItemIds = new List<int>
        {
            2673, // 松露虫
            1999, //帛斑蝶

            2436, //蓝水母
            2437, //绿水母
            2438, //粉水母

            2891, //金蝴蝶
            4340, //金蜻蜓
            2893, //金蚱蜢
            4362, //金瓢虫
            4419, //金水黾
            2895, //金蠕虫
        };

        DefaultProtectValuableBaitEnabled = true;

        ExtraCatchItemIds = new List<int>
        {
            5, //蘑菇
            72, //银币
            75, //坠落之星
            276, //仙人掌
            3093, //草药袋
            4345, //蠕虫罐头
        };
    }

    /// <summary>
    ///     将当前配置写入磁盘。
    /// </summary>
    public void Write()
    {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(FilePath, json);
    }

    /// <summary>
    ///     读取配置文件，若不存在则创建默认配置。
    /// </summary>
    public static Configuration Read()
    {
        if (!File.Exists(FilePath))
        {
            var NewConfig = new Configuration();
            NewConfig.Ints();
            new Configuration().Write();
            return NewConfig;
        }

        var jsonContent = File.ReadAllText(FilePath);
        return JsonConvert.DeserializeObject<Configuration>(jsonContent)!;
    }
}