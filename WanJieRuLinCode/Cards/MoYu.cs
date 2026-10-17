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
/// 墨域 —— 对所有敌人造成 12 伤害，本回合已打作祟则全体 +8 伤害（消耗）。
/// 升级：15 伤害 / +10。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MoYu : WanJieRuLinCardModel
{
    public MoYu() : base(2, CardType.Attack, CardRarity.Ancient, TargetType.AllEnemies)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, ValueProp.Move),
        new DamageVar("Conditional", 8m, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var baseDamage = IsUpgraded ? 15m : 12m;
        var conditional = ZuoSuiPlayedThisTurn ? (IsUpgraded ? 10m : 8m) : 0m;

        await DamageCmd.Attack(baseDamage + conditional)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);              // 12 → 15
        DynamicVars["Conditional"].UpgradeValueBy(2m);      // 8 → 10
    }
}
