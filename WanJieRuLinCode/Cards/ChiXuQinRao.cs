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
/// 给予 1 层易伤、2 层虚弱；造成 10 点伤害。升级后 14 点。
/// ★ 原为 2 费 14 伤害，按原版尺子偏高，现下修为 1 费 10 伤害。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ChiXuQinRao : WanJieRuLinCardModel
{
    public ChiXuQinRao() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        SetGhostQiCost(1);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move),
        ModCardVars.Int("Vulnerable", 1),
        ModCardVars.Int("Weak", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await ApplyTo<VulnerablePower>(choiceContext, cardPlay.Target, DynamicVars.GetIntOrDefault("Vulnerable", 1));
        await ApplyTo<WeakPower>(choiceContext, cardPlay.Target, DynamicVars.GetIntOrDefault("Weak", 2));
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);    }
}
