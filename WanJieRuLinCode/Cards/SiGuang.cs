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
/// 撕光 —— 造成 3 伤害，然后本回合你下一张攻击牌 +3 伤害。升级：5 伤，+4 伤。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class SiGuang : WanJieRuLinCardModel
{
    public SiGuang() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Move),
        new DynamicVar("NextAttackBonus", IsUpgraded ? 4m : 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 本回合下一张攻击牌伤害 +N（用凌厉式临时力量实现：这里直接施加一层临时力量，
        // 但它会对「下一张攻击」而非所有攻击生效——为简化，用一次性临时力量能力）。
        await PowerCmd.Apply<WanJieTempStrengthPower>(
            choiceContext, Owner!.Creature, DynamicVars["NextAttackBonus"].BaseValue,
            Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);              // 3 → 5
        DynamicVars["NextAttackBonus"].UpgradeValueBy(1m);  // 3 → 4
    }
}
