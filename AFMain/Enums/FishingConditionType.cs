namespace AutoFish.AFMain.Enums;

/// <summary>
///     钓鱼条件枚举，对应游戏中的 AFishingCondition。
/// </summary>
public enum FishingConditionType
{
    /// <summary>困难模式。</summary>
    HardMode,

    /// <summary>早期模式（非困难模式）。</summary>
    EarlyMode,

    /// <summary>在熔岩中。</summary>
    InLava,

    /// <summary>在蜂蜜中。</summary>
    InHoney,

    /// <summary>垃圾物品。</summary>
    Junk,

    /// <summary>宝箱。</summary>
    Crate,

    /// <summary>有敌怪生成。</summary>
    AnyEnemies,

    /// <summary>可以在熔岩中钓鱼。</summary>
    CanFishInLava,

    /// <summary>地牢（已击败骷髅王）。</summary>
    Dungeon,

    /// <summary>海滩。</summary>
    Beach,

    /// <summary>神圣之地。</summary>
    Hallow,

    /// <summary>发光蘑菇地。</summary>
    GlowingMushrooms,

    /// <summary>真正的沙漠。</summary>
    TrueDesert,

    /// <summary>真正的雪地。</summary>
    TrueSnow,

    /// <summary>Remix 世界。</summary>
    Remix,

    /// <summary>高度等级 = 0（天空）。</summary>
    Height0,

    /// <summary>高度等级 = 1（地表）。</summary>
    Height1,

    /// <summary>高度等级 = 2（地下）。</summary>
    Height2,

    /// <summary>高度等级 = 3（洞穴）。</summary>
    Height3,

    /// <summary>高度等级 = 1 或 2。</summary>
    Height1And2,

    /// <summary>高度等级 > 1。</summary>
    HeightAbove1,

    /// <summary>高度等级 >= 1。</summary>
    HeightAboveAnd1,

    /// <summary>高度等级 < 2。</summary>
    HeightUnder2,

    /// <summary>高度等级 > 2。</summary>
    HeightAbove2,

    /// <summary>在岩石层以下。</summary>
    UnderRockLayer,

    /// <summary>腐化之地。</summary>
    Corruption,

    /// <summary>猩红之地。</summary>
    Crimson,

    /// <summary>丛林。</summary>
    Jungle,

    /// <summary>雪地。</summary>
    Snow,

    /// <summary>沙漠。</summary>
    Desert,

    /// <summary>神圣沙漠。</summary>
    RolledHallowDesert,

    /// <summary>原版海洋。</summary>
    OriginalOcean,

    /// <summary>Remix 海洋。</summary>
    RemixOcean,

    /// <summary>海洋（原版或 Remix）。</summary>
    Ocean,

    /// <summary>水瓦片数量 > 1000。</summary>
    Water1000,

    /// <summary>血月。</summary>
    BloodMoon,

    /// <summary>未使用战斗手册。</summary>
    DidNotUseCombatBook
}
