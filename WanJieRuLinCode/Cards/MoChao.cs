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
/// 墨潮 —— 对所有敌人造成 7 点伤害。若本回合已打作祟，伤害 +5。升级：10 伤，条件 +7。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MoChao : WanJieRuLinCardModel
{
    public MoChao() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new DamageVar("Bonus", 5m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var damage = DynamicVars.Damage.BaseValue;

        if (ZuoSuiPlayedThisTurn)
        {
            damage += DynamicVars["Bonus"].BaseValue;
        }

        await DamageCmd.Attack(damage)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);     // 7 → 10
        DynamicVars["Bonus"].UpgradeValueBy(2m);   // 5 → 7
    }
}
