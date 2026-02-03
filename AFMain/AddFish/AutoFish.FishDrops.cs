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
        return GetMyItemDropType(context);
    }

    public int GetMyItemDropType(FishingContext context)
    {
        var resultItemType = 0;
        //我们的
        if (resultItemType == 0)
            resultItemType = My_TryGetItemDropType(context, CustomRuleList);
        //原版的
        if (resultItemType == 0)
            resultItemType = My_TryGetItemDropType(context, FishingConditionMapper.SystemRuleList);
        return resultItemType;
    }

    public int My_TryGetItemDropType(FishingContext context, FishDropRuleList ruleList) //没这个会导致无限递归
    {
        int resultItemType = 0;
        for (int index = 0; index < ruleList._rules.Count; ++index)
        {
            if (ruleList._rules[index].Attempt(context, out resultItemType))
                return resultItemType;
        }

        return 0;
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