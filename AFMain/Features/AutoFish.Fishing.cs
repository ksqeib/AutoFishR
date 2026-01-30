using AutoFish.Utils;
using Terraria;
using Terraria.GameContent.FishDropRules;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace AutoFish.AFMain;

public partial class AutoFish
{
    private void OnAI_061_FishingBobber(Projectile projectile,
        HookEvents.Terraria.Projectile.AI_061_FishingBobberEventArgs args)
    {
        HookUpdate(projectile);
        args.ContinueExecution = false;
    }

    /// <summary>
    ///     触发自动钓鱼，处理浮漂 AI 更新与收杆逻辑。原理：每次AI更新后尝试为玩家把鱼钓起来，并生成一个新的同样的弹射物
    /// </summary>
    private void HookUpdate(Projectile hook)
    {
        if (hook.owner < 0) return;
        if (hook.owner > Main.maxPlayers) return;
        if (!hook.active) return;
        if (!hook.bobber) return;
        if (!Config.PluginEnabled) return;
        if (!Config.GlobalAutoFishFeatureEnabled) return;

        var player = TShock.Players[hook.owner];
        if (player == null) return;
        if (!player.Active) return;

        var skipNonStackableLoot = Config.GlobalSkipNonStackableLoot &&
                                   HasFeaturePermission(player, "filter.unstackable");
        var blockMonsterCatch = Config.GlobalBlockMonsterCatch &&
                                HasFeaturePermission(player, "filter.monster");
        var skipFishingAnimation = Config.GlobalSkipFishingAnimation &&
                                   HasFeaturePermission(player, "skipanimation");
        var protectValuableBait = Config.GlobalProtectValuableBaitEnabled &&
                                  HasFeaturePermission(player, "bait.protect");

        // 从数据表中获取与玩家名字匹配的配置项
        var playerData = PlayerData.GetOrCreatePlayerData(player.Name, CreateDefaultPlayerData);
        //初次钓鱼提醒和未开启返回
        if (!playerData.AutoFishEnabled)
        {
            if (!HasFeaturePermission(player, "fish")) return;
            if (playerData.FirstFishHintShown) return;
            playerData.FirstFishHintShown = true;
            player.SendInfoMessage(Lang.T("firstFishHint"));
            return;
        }

        skipNonStackableLoot &= playerData.SkipNonStackableLoot;
        blockMonsterCatch &= playerData.BlockMonsterCatch;
        skipFishingAnimation &= playerData.SkipFishingAnimation;
        protectValuableBait &= playerData.ProtectValuableBaitEnabled;

        //负数时候为咬钩倒计时，说明上鱼了
        if (!(hook.ai[1] < 0)) return;

        player.TPlayer.Fishing_GetBait(out var baitPower, out var baitType);
        if (baitType == 0) return; //没有鱼饵，不要继续

        // 保护贵重鱼饵：将其移到背包末尾以避免被消耗
        if (protectValuableBait && Config.ValuableBaitItemIds.Contains(baitType))
            if (Tools.TrySwapValuableBaitToBack(player, baitType, Config.ValuableBaitItemIds,
                    out var fromSlot, out var toSlot, out var fromType, out var toType))
            {
                player.SendData(PacketTypes.PlayerSlot, "", player.Index, fromSlot);
                player.SendData(PacketTypes.PlayerSlot, "", player.Index, toSlot);
                var fromName = TShock.Utils.GetItemById(fromType).Name;
                var toName = TShock.Utils.GetItemById(toType).Name;
                Tools.SendGradientMessage(player,
                    Lang.T("protectBait.swap", fromName, toName, fromSlot, toSlot));
                ResetHook(hook);
                return;
            }
            else //就剩下一个了
            {
                var baitName = TShock.Utils.GetItemById(baitType).Name;
                Tools.SendGradientMessage(player, Lang.T("protectBait.lastOne", baitName));
                ResetHook(hook);
                player.SendData(PacketTypes.ProjectileDestroy, "", hook.whoAmI);
                return;
            }
        
        
        // 正常状态下与消耗模式下启用自动钓鱼
        if (Config.GlobalConsumptionModeEnabled)
        {
            //消耗模式判定
            if (!CanConsumeFish(player, playerData))
            {
                player.SendInfoMessage(Lang.T("consumption.lackItem"));
                ResetHook(hook);
                player.SendData(PacketTypes.ProjectileDestroy, "", hook.whoAmI);
                return;
            }
        }

        //修改钓鱼得到的东西
        //获得钓鱼物品方法
        var noCatch = true;
        var activePlayerCount = TShock.Players.Count(p => p != null && p.Active && p.IsLoggedIn);
        var dropLimit = Tools.GetLimit(activePlayerCount); //根据人数动态调整Limit
        var catchMonster = false;
        for (var count = 0; noCatch && count < dropLimit; count++)
        {
            var catchItem = false;
            //影子方法，获取物品啥的
            var context = MyFishingCheck(hook);


            var catchId = hook.localAI[1];

            if (Config.RandomLootEnabled) catchId = Random.Shared.Next(1, ItemID.Count);

            if (context.Fisher.rolledEnemySpawn > 0) //抓到怪物
            {
                if (blockMonsterCatch) continue; //不想抓怪物
                catchMonster = true;
                noCatch = false;
            }

            // 怪物生成使用localAI[1]，而物品则使用ai[1]，小于0情况无需处理，是刷血月怪
            if (context.Fisher.rolledItemDrop > 0)
            {
                catchItem = true;
                noCatch = false;
            }

            // 如果额外渔获有任何1个物品ID，则参与AI[1]
            if (noCatch)
            {
                //钓额外渔获
                if (Config.ExtraCatchItemIds.Any())
                {
                    catchId = Config.ExtraCatchItemIds[Main.rand.Next(Config.ExtraCatchItemIds.Count)];
                    noCatch = false;
                    catchItem = true;
                }
            }
            //想给额外渔获加点怪物

            //抓到物品
            if (catchItem && skipNonStackableLoot) //不想抓不可堆叠堆叠物品
            {
                var item = new Item();
                item.SetDefaults((int)catchId);
                if (item.maxStack == 1) continue;
            }

            if (noCatch) continue; //真没抓到

            hook.localAI[1] = catchId; //数值置回
            break; //抓到就不应该继续判断
        }

        if (noCatch)
        {
            ResetHook(hook);
            return; //没抓到，不抬杆
        }

        //设置为收杆状态，没用
        // hook.ai[0] = 1.0f;

        var nowBaitType = (int)hook.localAI[2];

        // 让服务器扣饵料
        var locate = LocateBait(player, nowBaitType);
        var pull = player.TPlayer.ItemCheck_CheckFishingBobber_ConsumeBait(hook,
            out var baitUsed);
        if (nowBaitType != baitUsed)
        {
            player.SendMessage("鱼饵不一致", Colors.CurrentLiquidColor);
            return;
        }

        if (!pull) return; //说明鱼饵没了，不能继续，否则可能会卡bug
        //Buff更新
        if (playerData.BuffEnabled)
            BuffUpdate(player);

        //原版收杆函数，这里会使得  bobber.ai[1] = bobber.localAI[1];，必须调用此函数，否则杆子会爆一堆弹幕，并且鱼饵会全不见，也是刷怪的函数
        player.TPlayer.ItemCheck_CheckFishingBobber_PullBobber(hook, baitUsed);
        // 同步玩家背包
        player.SendData(PacketTypes.PlayerSlot, "", player.Index, locate);

        // 原版给东西的代码，在kill函数，会把ai[1]给玩家
        // 这里发的是连续弹幕 避免线断 因为弹幕是不需要玩家物理点击来触发收杆的，但是服务端和客户端概率测算不一样，会导致服务器扣了饵料，但是客户端没扣
        //New会强制写AI状态，所以Destroy即使调用kill也没有，因为对端没有物品
        //应该发一个带ai[1]然后直接销毁，就可以触发客户端的收取函数，而不是继续New。
        var origPos = hook.position;
        //往玩家的二十倍对角方向飞，常规方法无法销毁，粒子动画阻塞
        var pos = CalcNewPos(player, hook);
        hook.position = pos;
        player.SendData(PacketTypes.ProjectileNew, "", hook.whoAmI);
        //服务器的netMod为2

        if (!catchMonster) //抓到怪物触发会导致刷鱼漂，这里是重新设置溅射物
        {
            spawnHook(player, hook, origPos);
            //多钩钓鱼代码
            AddMultiHook(player, hook, origPos);
        }

        if (skipFishingAnimation) //跳过上鱼动画
            player.SendData(PacketTypes.ProjectileDestroy, "", hook.whoAmI);
    }


    private static FishingContext MyFishingCheck(Projectile hook)
    {
        FishingContext context = Projectile._context;
        if (hook.TryBuildFishingContext(context))
        {
            int num = (context.Fisher.fishingLevel + 75) / 2;
            if (Main.rand.Next(100) <= num)
            {
                hook.SetFishingCheckResults(ref context.Fisher);
            }
        }

        return context;
    }


    private static Vector2 CalcNewPos(TSPlayer player, Projectile hook)
    {
        var playerPos = player.TPlayer.position;
        var hookPos = hook.position;

        Vector2 direction = hookPos - playerPos;

        // 3. 获取当前的实际距离
        float distance = direction.Length();

        // 4. 异常处理：如果距离为0（重合），无法确定方向，直接返回玩家坐标
        if (distance == 0f)
        {
            return playerPos;
        }

        // 5. 归一化向量 (Normalize)
        // 这会将向量的长度变为 1，但方向保持不变。
        // 这样我们就剥离了“距离”，只保留了“方向”。
        direction.Normalize();

        // 6. 限制距离范围 (Clamp)
        // 我们只想要 900 到 3000 之间的长度
        float targetDistance = distance; // 默认使用原距离
        if (targetDistance < 900f)
        {
            targetDistance = 1000f;
        }
        else if (targetDistance > 3000f)
        {
            targetDistance = 2000f;
        }

        // 如果在中间，就保持原样不动
        // 7. 计算最终坐标
        // 公式：玩家坐标 + (单位方向向量 * 目标距离)
        var pos = playerPos + direction * targetDistance;

        return pos;
    }


    private static void spawnHook(TSPlayer player, Projectile hook, Vector2 pos)
    {
        var velocity = new Vector2(0, 0);
        // var pos = new Vector2(hook.position.X, hook.position.Y + 3);
        var index = SpawnProjectile.NewProjectile(
            Main.projectile[hook.whoAmI].GetProjectileSource_FromThis(),
            pos, velocity, hook.type, 0, 0,
            hook.owner);
        player.SendData(PacketTypes.ProjectileNew, "", index);
    }

    private static void ResetHook(Projectile projectile)
    {
        //设置成没上鱼，无状态
        projectile.ai[1] = 0;
        //清空进度
        projectile.localAI[1] = 0;
        //原版岩浆类似逻辑，加点进度
        projectile.localAI[1] += 240f;
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

    private static void BuffUpdate(TSPlayer player)
    {
        if (!Config.GlobalBuffFeatureEnabled) return;
        foreach (var buff in Config.BuffDurations)
            player.SetBuff(buff.Key, buff.Value);
    }
}