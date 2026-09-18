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
/// 我若为鬼 —— 技能牌 1 能量：下回合获得的能量转为鬼气值，下回合抽 2 张牌。消耗。
/// 升级后抽 3 张牌。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class WoRuoWeiGui : WanJieRuLinCardModel
{
    private const int CardsToDraw = 2;

    public WoRuoWeiGui() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(CardsToDraw)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<EnergyToGhostQiPower>(choiceContext, 1m);
        await ApplySelf<DrawCardsNextTurnPower>(
            choiceContext, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
