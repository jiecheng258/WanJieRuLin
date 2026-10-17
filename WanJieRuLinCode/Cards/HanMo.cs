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
/// 寒墨 —— 对所有敌人造成 5 点伤害，施加 1 层虚弱和 1 层易伤。升级：7 伤，2 虚 2 易。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class HanMo : WanJieRuLinCardModel
{
    public HanMo() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new DynamicVar("Weak", 1m),
        new DynamicVar("Vulnerable", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);

        var stacks = IsUpgraded ? 2m : 1m;
        await ApplyToAll<WeakPower>(choiceContext, stacks);
        await ApplyToAll<VulnerablePower>(choiceContext, stacks);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);        // 5 → 7
        DynamicVars["Weak"].UpgradeValueBy(1m);       // 1 → 2
        DynamicVars["Vulnerable"].UpgradeValueBy(1m); // 1 → 2
    }
}
