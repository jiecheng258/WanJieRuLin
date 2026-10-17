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
/// 鬼壁 —— 获得 8 格挡，若本回合已打作祟则本回合所有敌人虚弱 1。升级：11 格挡，虚弱 2。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class GuiBi : WanJieRuLinCardModel
{
    public GuiBi() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(8m, ValueProp.Move),
        new DynamicVar("Weak", IsUpgraded ? 2m : 1m)
    ];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlock(choiceContext, DynamicVars.Block.BaseValue);

        if (ZuoSuiPlayedThisTurn)
        {
            await ApplyToAll<WeakPower>(choiceContext, DynamicVars["Weak"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m); // 8 → 11
        DynamicVars["Weak"].UpgradeValueBy(1m); // 1 → 2
    }
}
