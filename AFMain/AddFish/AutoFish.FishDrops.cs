using AutoFish.AFMain.Enums;
using Terraria.GameContent.FishDropRules;
using FishingContext = Terraria.GameContent.FishDropRules.FishingContext;

namespace AutoFish.AFMain;

public partial class AutoFish
{
    public static readonly FishDropRuleList RuleList = new();

    public void AddFishRule(FishRarityCondition tier, int chanceNominator, int chanceDenominator, int[] itemTypes,
        params AFishingCondition[] conditions)
    {
        FishDropRule rule = new FishDropRule
        {
            PossibleItems = itemTypes,
            ChanceNumerator = chanceNominator,
            ChanceDenominator = chanceDenominator,
            Rarity = tier,
            Conditions = conditions
        };
        RuleList.Add(rule);
    }

    protected void AddFishRuleWithHardmode(FishRarityCondition tier, int chanceDenominator, int itemTypeEarly,
        int itemTypeHard, params AFishingCondition[] conditions)
    {
        FishDropRule fishDropRule = new FishDropRule();
        fishDropRule.PossibleItems = new int[1] { itemTypeEarly };
        fishDropRule.ChanceNumerator = 1;
        fishDropRule.ChanceDenominator = chanceDenominator;
        fishDropRule.Rarity = tier;
        fishDropRule.Conditions = FishingConditionMapper.Populator.Join(conditions,
            FishingConditionMapper.GetCondition(FishingConditionType.EarlyMode));
        FishDropRule rule = fishDropRule;
        RuleList.Add(rule);
        fishDropRule = new FishDropRule();
        fishDropRule.PossibleItems = new int[1] { itemTypeHard };
        fishDropRule.ChanceNumerator = 1;
        fishDropRule.ChanceDenominator = chanceDenominator;
        fishDropRule.Rarity = tier;
        fishDropRule.Conditions = FishingConditionMapper.Populator.Join(conditions,
            FishingConditionMapper.GetCondition(FishingConditionType.HardMode));
        FishDropRule rule2 = fishDropRule;
        RuleList.Add(rule2);
    }

    public int my_TryGetItemDropType(
        On.Terraria.GameContent.FishDropRules.FishDropRuleList.orig_TryGetItemDropType orig,
        Terraria.GameContent.FishDropRules.FishDropRuleList self,
        FishingContext context)
    {
        var resultItemType = 0;

        //原版的
        resultItemType = FishingConditionMapper.RuleList.TryGetItemDropType(context);
        return resultItemType;
    }

    public int GetMyItemDropType(FishingContext context)
    {
        var resultItemType = 0;
        //我们的
        if (resultItemType == 0)
            resultItemType = RuleList.TryGetItemDropType(context);
        //原版的
        if (resultItemType == 0)
            resultItemType = FishingConditionMapper.RuleList.TryGetItemDropType(context);
        return resultItemType;
    }

    //注册上去，做一个自己的RuleList就好了
    public void RegisterToFishDB()
    {
        // On.Terraria.GameContent.FishDropRules.FishDropRuleList.TryGetItemDropType += my_TryGetItemDropType;
    }

    public void UnRegisterToFishDB()
    {
        // On.Terraria.GameContent.FishDropRules.FishDropRuleList.TryGetItemDropType -= my_TryGetItemDropType;
    }
}