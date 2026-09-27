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
/// 获得 5 点格挡。升级后 8 点。
/// </summary>
[RegisterCharacterStarterCard(typeof(WanJieRuLinCharacter), 4)]
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class FangYu : WanJieRuLinCardModel
{
    public FangYu() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlock(choiceContext, DynamicVars.Block.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);    }
}
