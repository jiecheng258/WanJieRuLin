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
/// 墨海 —— 对所有敌人造成 8 伤害，给予 3 层易伤。升级：11 伤害，4 易伤。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MoHai : WanJieRuLinCardModel
{
    public MoHai() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
        new DynamicVar("Vulnerable", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DealDamageToAll(choiceContext, DynamicVars.Damage.BaseValue);
        await ApplyToAll<VulnerablePower>(choiceContext, IsUpgraded ? 4m : 3m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);         // 8 → 11
        DynamicVars["Vulnerable"].UpgradeValueBy(1m);  // 3 → 4
    }
}
