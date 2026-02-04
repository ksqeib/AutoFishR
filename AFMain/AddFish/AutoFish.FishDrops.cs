using AutoFish.AFMain.Enums;
using Terraria.GameContent.FishDropRules;
using FishingContext = Terraria.GameContent.FishDropRules.FishingContext;

namespace AutoFish.AFMain;

public partial class AutoFish
{
    public static readonly FishDropRuleList CustomRuleList = new();

    public int my_TryGetItemDropType(
        On.Terraria.GameContent.FishDropRules.FishDropRuleList.orig_TryGetItemDropType orig,
        Terraria.GameContent.FishDropRules.FishDropRuleList self,
        FishingContext context)
    {
        var resultItemType = 0;
        //我们的
        if (resultItemType == 0 && Config.EnableCustomFishRules)
            resultItemType = orig(CustomRuleList, context);
        //原版的
        if (resultItemType == 0 && Config.EnableVanillaFishRules)
            resultItemType = orig(FishingConditionMapper.SystemRuleList, context);
        return resultItemType;
    }
    
    //注册上去，做一个自己的RuleList就好了
    public void RegisterToFishDB()
    {
        On.Terraria.GameContent.FishDropRules.FishDropRuleList.TryGetItemDropType += my_TryGetItemDropType;
    }

    public void UnRegisterToFishDB()
    {
        On.Terraria.GameContent.FishDropRules.FishDropRuleList.TryGetItemDropType -= my_TryGetItemDropType;
    }
}