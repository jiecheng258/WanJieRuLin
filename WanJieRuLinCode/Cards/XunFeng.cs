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
/// 巡风 —— 获得 6 点格挡。若本回合已打作祟，本回合你所有攻击 +3 伤害。升级：8 挡，攻击 +5。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class XunFeng : WanJieRuLinCardModel
{
    public XunFeng() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(6m, ValueProp.Move),
        new DynamicVar("AttackBonus", 3m)
    ];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlock(choiceContext, DynamicVars.Block.BaseValue);

        if (ZuoSuiPlayedThisTurn && Owner is { } p)
        {
            await PowerCmd.Apply<WanJieTempStrengthPower>(
                choiceContext, p.Creature, IsUpgraded ? 5m : 3m, p.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);        // 6 → 8
        DynamicVars["AttackBonus"].UpgradeValueBy(2m); // 3 → 5
    }
}
