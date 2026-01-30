using AutoFish.Utils;
using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;

namespace AutoFish.AFMain;

public partial class AutoFish
{
    /// <summary>
    ///     处理多线钓鱼，派生额外的鱼线弹幕。
    /// </summary>
    public void AddMultiHook(TSPlayer player, Projectile oldHook, Vector2 pos)
    {
        if (!Config.GlobalMultiHookFeatureEnabled) return;

        // 从数据表中获取与玩家名字匹配的配置项
        var playerData = PlayerData.GetOrCreatePlayerData(player.Name, CreateDefaultPlayerData);
        if (!playerData.MultiHookEnabled) return;

        var hookCount = Main.projectile.Count(p => p.active && p.owner == oldHook.owner && p.bobber); // 浮漂计数
        //数量检测
        if (hookCount > Config.GlobalMultiHookMaxNum - 1) return;
        if (hookCount > playerData.HookMaxNum - 1) return;

        spawnHookForNew(player, oldHook, pos);
    }

    public static void spawnHookForNew(TSPlayer player, Projectile hook, Vector2 pos)
    {
        var guid = Guid.NewGuid().ToString();
        var velocity = new Vector2(0, 0);
        // var pos = new Vector2(hook.position.X, hook.position.Y + 3);
        var index = SpawnProjectile.NewProjectile(
            Main.projectile[hook.whoAmI].GetProjectileSource_FromThis(),
            pos, velocity, hook.type, 0, 0,
            hook.owner, 0, 0, 0, -1, guid);
        player.SendData(PacketTypes.ProjectileNew, "", index);
    }
}