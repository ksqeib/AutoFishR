using AutoFish.Utils;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace AutoFish.AFMain;

public partial class AutoFish
{
    /// <summary>
    ///     触发自动钓鱼，处理浮漂 AI 更新与收杆逻辑。原理：每次AI更新后尝试为玩家把鱼钓起来，并生成一个新的同样的弹射物
    /// </summary>
    private void ProjectAiUpdate(ProjectileAiUpdateEventArgs args)
    {
        if (args.Projectile.owner < 0) return;
        if (args.Projectile.owner > Main.maxPlayers) return;
        if (!args.Projectile.active) return;
        if (!args.Projectile.bobber) return;
        if (!Config.PluginEnabled) return;
        if (!Config.GlobalAutoFishFeatureEnabled) return;

        var player = TShock.Players[args.Projectile.owner];
        if (player == null) return;
        if (!player.Active) return;

        var skipNonStackableLoot = Config.GlobalSkipNonStackableLoot &&
                                   HasFeaturePermission(player, "autofish.filter.unstackable");
        var blockMonsterCatch = Config.GlobalBlockMonsterCatch &&
                                HasFeaturePermission(player, "autofish.filter.monster");
        var skipFishingAnimation = Config.GlobalSkipFishingAnimation &&
                                   HasFeaturePermission(player, "autofish.skipanimation");

        // 从数据表中获取与玩家名字匹配的配置项
        var playerData = PlayerData.GetOrCreatePlayerData(player.Name, CreateDefaultPlayerData);
        if (!playerData.AutoFishEnabled) return;

        skipNonStackableLoot &= playerData.SkipNonStackableLoot;
        blockMonsterCatch &= playerData.BlockMonsterCatch;
        skipFishingAnimation &= playerData.SkipFishingAnimation;

        // 正常状态下与消耗模式下启用自动钓鱼
        if (Config.GlobalConsumptionModeEnabled && !playerData.ConsumptionEnabled) return;

        //负数时候为咬钩倒计时，说明上鱼了
        if (!(args.Projectile.ai[1] < 0)) return;
        
        player.TPlayer.Fishing_GetBait(out var baitPower, out var baitType);
        if (baitType == 0) return; //没有鱼饵，不要继续

        //松露虫 判断一下玩家是否在海边
        if (baitType == 2673 && player.X / 16 == Main.oceanBG && player.Y / 16 == Main.oceanBG)
        {
            args.Projectile.ai[1] = 0;
            player.SendData(PacketTypes.ProjectileNew, "", args.Projectile.whoAmI);
            return;
        }

        //修改钓鱼得到的东西
        //获得钓鱼物品方法
        var noCatch = true;
        var activePlayerCount = TShock.Players.Count(p => p != null && p.Active && p.IsLoggedIn);
        var dropLimit = Tools.GetLimit(activePlayerCount); //根据人数动态调整Limit
        var caughtMonster = false;
        for (var count = 0; noCatch && count < dropLimit; count++)
        {
            //61就是直接调用AI_061_FishingBobber
            //原版方法，获取物品啥的
            args.Projectile.FishingCheck();

            var catchId = args.Projectile.localAI[1];

            if (Config.RandomLootEnabled)
            {
                catchId = Random.Shared.Next(1, ItemID.Count);
            }

            // 如果额外渔获有任何1个物品ID，则参与AI[1]
            if (catchId == 0) //钓不到就给额外的
                if (Config.ExtraCatchItemIds.Any())
                    catchId = Config.ExtraCatchItemIds[Main.rand.Next(Config.ExtraCatchItemIds.Count)];
            //想给额外渔获加点怪物

            if (catchId < 0) //抓到怪物
            {
                if (blockMonsterCatch) continue; //不想抓怪物
                caughtMonster = true;
            }

            // 怪物生成使用localAI[1]，而物品则使用ai[1]，小于0情况无需处理，是刷血月怪
            if (catchId > 0) //抓到物品
            {
                if (skipNonStackableLoot) //不想抓不可堆叠堆叠物品
                {
                    var item = new Item();
                    item.SetDefaults((int)catchId);
                    if (item.maxStack == 1) continue;
                }
            }

            noCatch = catchId == 0;//是否空军
            if (noCatch) continue;
            
            args.Projectile.localAI[1] = catchId; //数值置回
            break; //抓到就不应该继续判断
        }

        if (noCatch)
        {
            //清空进度
            args.Projectile.localAI[1] = 0;
            //原版岩浆类似逻辑，加点进度
            args.Projectile.localAI[1] += 240f;
            return; //没抓到，不抬杆
        }

        //设置为收杆状态
        args.Projectile.ai[0] = 1.0f;

        // 让服务器扣饵料
        var locate = LocateBait(player, baitType);
        player.TPlayer.ItemCheck_CheckFishingBobber_PickAndConsumeBait(args.Projectile, out var pull,
            out var baitUsed);
        if (!pull) return; //说明鱼饵没了，不能继续，否则可能会卡bug
        //原版收杆函数，这里会使得  bobber.ai[1] = bobber.localAI[1];，必须调用此函数，否则杆子会爆一堆弹幕，并且鱼饵会全不见
        player.TPlayer.ItemCheck_CheckFishingBobber_PullBobber(args.Projectile, baitUsed);
        // 同步玩家背包
        player.SendData(PacketTypes.PlayerSlot, "", player.Index, locate);

        // 原版给东西的代码，在kill函数，会把ai[1]给玩家
        // 这里发的是连续弹幕 避免线断 因为弹幕是不需要玩家物理点击来触发收杆的，但是服务端和客户端概率测算不一样，会导致服务器扣了饵料，但是客户端没扣
        player.SendData(PacketTypes.ProjectileNew, "", args.Projectile.whoAmI);

        if (!caughtMonster) //抓到怪物触发会导致刷鱼漂，这里是重新设置溅射物
        {
            var velocity = new Vector2(0, 0);
            var pos = new Vector2(args.Projectile.position.X, args.Projectile.position.Y + 3);
            var index = SpawnProjectile.NewProjectile(
                Main.projectile[args.Projectile.whoAmI].GetProjectileSource_FromThis(),
                pos, velocity, args.Projectile.type, 0, 0,
                args.Projectile.owner);
            player.SendData(PacketTypes.ProjectileNew, "", index);
        }

        if (skipFishingAnimation)
        {
            //跳过上鱼动画
            player.SendData(PacketTypes.ProjectileDestroy, "", args.Projectile.whoAmI);
        }
    }

    private static int LocateBait(TSPlayer player, int baitUsed)
    {
        // 更新玩家背包 使用饵料信息
        for (var i = 0; i < player.TPlayer.inventory.Length; i++)
        {
            var inventorySlot = player.TPlayer.inventory[i];
            // 玩家饵料（指的是你手上鱼竿上的那个数字），使用的饵料是背包里的物品
            if (inventorySlot.bait <= 0 || baitUsed != inventorySlot.type) continue;
            return i;
        }

        return 0;
    }
}