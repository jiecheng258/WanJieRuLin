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
/// 墨气 —— 获得 2 鬼气，获得 5 点格挡。升级：3 气，8 挡。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MoQi : WanJieRuLinCardModel
{
    public MoQi() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        GhostQiGainVarOf(2),
        new BlockVar(5m, ValueProp.Move)
    ];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainGhostQi(IsUpgraded ? 3 : 2);
        await GainBlock(choiceContext, DynamicVars.Block.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[GhostQiGainVar].UpgradeValueBy(1m); // 2 → 3
        DynamicVars.Block.UpgradeValueBy(3m);           // 5 → 8
    }
}
