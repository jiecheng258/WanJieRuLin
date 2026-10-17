using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 灵犀 —— 消耗。本回合你下一张牌费用 -3。升级：-4。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class LingXi : WanJieRuLinCardModel
{
    public LingXi() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Reduction", 3m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 用 MoYiNextCardPower 承载「下一张牌减费」，减费额度 = Amount（3，升级 4）
        await ApplySelf<MoYiNextCardPower>(choiceContext, IsUpgraded ? 4m : 3m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Reduction"].UpgradeValueBy(1m); // 3 → 4
    }
}
