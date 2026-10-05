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
/// ★ **线** —— 获得 9 点格挡，获得 1 点[gold]力道[/gold]。升级后 9 点格挡。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class BiZhi : WanJieRuLinCardModel
{
    public BiZhi() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Line;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(9m, ValueProp.Move),
        ModCardVars.Int("Force", 1)
    ];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlock(choiceContext, DynamicVars.Block.BaseValue);
        await ApplySelf<LiDaoPower>(choiceContext, DynamicVars.GetIntOrDefault("Force", 1));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);    }
}
