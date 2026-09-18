using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 我若为神 —— 技能牌 1 能量：下回合获得的鬼气值转换为能量，下回合第 1 张牌可以免费打出。消耗。
/// 升级后下回合可以免费打出 2 张牌。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class WoRuoWeiShen : WanJieRuLinCardModel
{
    private const int FreeCards = 1;

    public WoRuoWeiShen() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("FreeCards", FreeCards)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<GhostQiToEnergyPower>(choiceContext, 1m);
        await ApplySelf<FreePowerPower>(
            choiceContext, DynamicVars.GetIntOrDefault("FreeCards", FreeCards));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FreeCards"].UpgradeValueBy(1);
    }
}
