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

        //检测是不是生成，是生成boss就不钓起来
        if (!(args.Projectile.ai[1] < 0)) return;

        args.Projectile.ai[0] = 1.0f;

        var fishingConditions = player.TPlayer.GetFishingConditions();
        if (fishingConditions.BaitItemType == 0) //没有鱼饵，不要继续
        {
            return;
        }

        //松露虫 判断一下玩家是否在海边
        if (fishingConditions.BaitItemType == 2673 && player.X / 16 == Main.oceanBG && player.Y / 16 == Main.oceanBG)
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

            if (catchId < 0)
            {
                if (blockMonsterCatch)
                {
                    catchId = 0;
                    args.Projectile.localAI[1] = 0;
                    continue;
                }

                caughtMonster = true;
            }

            // 怪物生成使用localAI[1]，而物品则使用ai[1]，小于0情况无需处理，是刷血月怪
            if (catchId > 0)
            {
                if (skipNonStackableLoot)
                {
                    var item = new Item();
                    item.SetDefaults((int)catchId);
                    if (item.maxStack == 1)
                    {
                        catchId = 0;
                        args.Projectile.localAI[1] = 0;
                        continue;
                    }
                }

                //ai[1] = localAI[1]
                // args.Projectile.ai[1] = catchId;
            }

            if (Config.RandomLootEnabled)
            {
                catchId = Random.Shared.Next(1, ItemID.Count);
            }

            // 如果额外渔获有任何1个物品ID，则参与AI[1]
            if (Config.ExtraCatchItemIds.Any())
                if (catchId == 0) //钓不到就给额外的
                    catchId = Config.ExtraCatchItemIds[Main.rand.Next(Config.ExtraCatchItemIds.Count)];

            noCatch = catchId == 0;
            if (!noCatch)
            {
                args.Projectile.localAI[1] = catchId; //数值置回
                break; //抓到就不应该继续判断
            }
        }

        if (noCatch) return; //没抓到，不抬杆

        // 让服务器扣饵料
        var locate = LocateBait(player, fishingConditions.BaitItemType);
        player.TPlayer.ItemCheck_CheckFishingBobber_PickAndConsumeBait(args.Projectile, out var pull,
            out var baitUsed);
        if (!pull) return; //说明鱼饵没了，不能继续，否则可能会卡bug
        //原版收杆函数，这里会使得  bobber.ai[1] = bobber.localAI[1];
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