using AutoFish.Data;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using Projectile = HookEvents.Terraria.Projectile;

namespace AutoFish.AFMain;

[ApiVersion(2, 1)]
public partial class AutoFish : TerrariaPlugin
{
    public const string PermissionPrefix = "autofish.";
    public const string AdminPermission = $"{PermissionPrefix}admin";
    public const string CommonPermission = $"{PermissionPrefix}common";
    public const string DenyPermissionPrefix = $"{PermissionPrefix}no.";

    /// <summary>全局配置实例。</summary>
    internal static Configuration Config = new();

    /// <summary>玩家数据集合。</summary>
    internal static AFPlayerData PlayerData = new();

    /// <summary>
    ///     创建插件实例。
    /// </summary>
    public AutoFish(Main game) : base(game)
    {
    }

    /// <summary>插件名称。</summary>
    public override string Name => "自动钓鱼R";

    /// <summary>插件作者。</summary>
    public override string Author => "ksqeib 羽学 少司命";

    /// <summary>插件版本。</summary>
    public override Version Version => new(1, 4, 7);

    /// <summary>插件描述。</summary>
    public override string Description => "青山常伴绿水，燕雀已是南飞";

    /// <summary>
    ///     默认玩家数据工厂，基于当前配置初始化。
    /// </summary>
    internal static AFPlayerData.ItemData CreateDefaultPlayerData(string playerName)
    {
        // Attempt to resolve current player to seed defaults from permissions
        var player = TShock.Players.FirstOrDefault(p => p != null && p.Active &&
                                                        p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));

        var canBuff = HasFeaturePermission(player, "buff");
        var canMulti = HasFeaturePermission(player, "multihook");
        var canFish = HasFeaturePermission(player, "fish");
        var canSkipNonStackable = HasFeaturePermission(player, "filter.unstackable");
        var canBlockMonster = HasFeaturePermission(player, "filter.monster");
        var canSkipAnimation = HasFeaturePermission(player, "skipanimation");
        var canBlockQuestFish = HasFeaturePermission(player, "filter.quest");
        var canProtectBait = HasFeaturePermission(player, "bait.protect");

        var defaultAutoFish = Config.DefaultAutoFishEnabled && canFish;
        var defaultBuff = Config.DefaultBuffEnabled && canBuff;
        var defaultMulti = Config.DefaultMultiHookEnabled && canMulti;
        var defaultSkipNonStackable = Config.GlobalSkipNonStackableLoot && Config.DefaultSkipNonStackableLoot &&
                                      canSkipNonStackable;
        var defaultBlockMonster = Config.GlobalBlockMonsterCatch && Config.DefaultBlockMonsterCatch &&
                                  canBlockMonster;
        var defaultSkipAnimation = Config.GlobalSkipFishingAnimation && Config.DefaultSkipFishingAnimation &&
                                   canSkipAnimation;
        var defaultBlockQuestFish = Config.GlobalBlockQuestFish && Config.DefaultBlockQuestFish &&
                                    canBlockQuestFish;
        var defaultProtectBait = Config.GlobalProtectValuableBaitEnabled && Config.DefaultProtectValuableBaitEnabled &&
                                 canProtectBait;

        return new AFPlayerData.ItemData
        {
            Name = playerName,
            AutoFishEnabled = defaultAutoFish,
            BuffEnabled = defaultBuff,
            HookMaxNum = Config.GlobalMultiHookMaxNum,
            MultiHookEnabled = defaultMulti,
            SkipNonStackableLoot = defaultSkipNonStackable,
            BlockMonsterCatch = defaultBlockMonster,
            SkipFishingAnimation = defaultSkipAnimation,
            BlockQuestFish = defaultBlockQuestFish,
            ProtectValuableBaitEnabled = defaultProtectBait,
            FirstFishHintShown = false
        };
    }

    /// <summary>统一的权限检查，支持 admin 全覆盖、common 通用以及显式负权限。</summary>
    internal static bool HasFeaturePermission(TSPlayer? player, string featureKey, bool allowCommon = true)
    {
        if (player == null) return false;

        if (player.HasPermission(AdminPermission)) return true;

        var denyPermission = $"{DenyPermissionPrefix}{featureKey}";
        if (player.HasPermission(denyPermission)) return false;

        if (allowCommon && player.HasPermission(CommonPermission)) return true;

        var allowPermission = $"{PermissionPrefix}{featureKey}";
        return player.HasPermission(allowPermission);
    }

    /// <summary>
    ///     插件初始化，注册事件和命令。
    /// </summary>
    public override void Initialize()
    {
        LoadConfig();
        GeneralHooks.ReloadEvent += ReloadConfig;
        //容易出bug，还是OTAPI精准打击吧
        // ServerApi.Hooks.ProjectileAIUpdate.Register(this, ProjectAiUpdate);
        Projectile.AI_061_FishingBobber += OnAI_061_FishingBobber;
        TShockAPI.Commands.ChatCommands.Add(new Command(new List<string> { "autofish", CommonPermission },
            Commands.Afs, "af", "autofish"));
        TShockAPI.Commands.ChatCommands.Add(new Command(AdminPermission, Commands.Afa, "afa", "autofishadmin"));
        RegisterToFishDB();
    }

    /// <summary>
    ///     释放插件，注销事件与命令。
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            GeneralHooks.ReloadEvent -= ReloadConfig;
            // ServerApi.Hooks.ProjectileAIUpdate.Deregister(this, ProjectAiUpdate);
            Projectile.AI_061_FishingBobber -= OnAI_061_FishingBobber;
            TShockAPI.Commands.ChatCommands.RemoveAll(x => x.CommandDelegate == Commands.Afs);
            TShockAPI.Commands.ChatCommands.RemoveAll(x => x.CommandDelegate == Commands.Afa);
            UnRegisterToFishDB();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    ///     处理 /reload 触发的配置重载。
    /// </summary>
    private static void ReloadConfig(ReloadEventArgs args)
    {
        LoadConfig();
        args.Player.SendInfoMessage(Lang.T("reload.done"));
    }

    /// <summary>
    ///     读取并落盘配置。
    /// </summary>
    private static void LoadConfig()
    {
        Config = Configuration.Read();
        Lang.Load(Config.Language);
        LoadCustomFishRules();
    }

    /// <summary>
    ///     从配置加载自定义钓鱼规则。
    /// </summary>
    private static void LoadCustomFishRules()
    {
        // 清空现有的自定义规则列表
        CustomRuleList._rules.Clear();

        if (Config.CustomFishRules == null || Config.CustomFishRules.Count == 0)
        {
            return;
        }

        try
        {
            var loadedCount = Enums.FishDropRuleExporter.LoadCustomRulesToList(
                Config.CustomFishRules,
                CustomRuleList);

            if (loadedCount > 0)
            {
                TShock.Log.ConsoleInfo($"[AutoFish] 已加载 {loadedCount} 条自定义钓鱼规则");
            }
        }
        catch (Exception ex)
        {
            TShock.Log.Error($"[AutoFish] 加载自定义钓鱼规则失败: {ex.Message}");
        }
    }
}