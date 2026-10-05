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
using WanJieRuLin.Aspects;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 先古 —— 0 费：获得 2 点能量。[消耗]
/// 代价：已计入「斑驳乱点」的计数。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class WuMingZhiShi : WanJieRuLinCardModel
{
    public WuMingZhiShi() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Point;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Energy", 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PointHit(choiceContext);
        await GainEnergy(DynamicVars.GetIntOrDefault("Energy", 2));
    }

    protected override void OnUpgrade()
    {
    }
}
