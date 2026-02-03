using Terraria.GameContent.FishDropRules;
using FishingContext = Terraria.GameContent.FishDropRules.FishingContext;

namespace AutoFish.AFMain;

public partial class AutoFish
{
    private List<FishDropRule> _myFishDropRules = [];
    
    public void Add(FishRarityCondition tier, int chanceNominator, int chanceDenominator, int[] itemTypes, params AFishingCondition[] conditions)
    {
        FishDropRule rule = new FishDropRule
        {
            PossibleItems = itemTypes,
            ChanceNumerator = chanceNominator,
            ChanceDenominator = chanceDenominator,
            Rarity = tier,
            Conditions = conditions
        };
        _myFishDropRules.Add(rule);
    }

    public int my_TryGetItemDropType(
        On.Terraria.GameContent.FishDropRules.FishDropRuleList.orig_TryGetItemDropType orig,
        Terraria.GameContent.FishDropRules.FishDropRuleList self,
        FishingContext context)
    {
        var resultItemType = 0;
        return _myFishDropRules.Any(t => t.Attempt(context, out resultItemType)) ? resultItemType : orig(self, context);
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