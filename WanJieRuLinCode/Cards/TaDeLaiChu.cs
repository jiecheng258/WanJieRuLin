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
/// 线 —— 获得 8 点格挡，获得 2 点临时力量。
/// 她的身世没人说得清，但她说的话总能让人多一分力气。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class TaDeLaiChu : WanJieRuLinCardModel
{
    public TaDeLaiChu() : base(1, CardType.Skill, CardRarity.Event, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Line;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(8m, ValueProp.Move),
        ModCardVars.Int("TempStr", 2)
    ];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await LineHit(choiceContext);
        await GainBlock(choiceContext, DynamicVars.Block.BaseValue);
        await ApplySelf<WanJieTempStrengthPower>(choiceContext, DynamicVars.GetIntOrDefault("TempStr", 2));
    }

    protected override void OnUpgrade()
    {
    }
}
