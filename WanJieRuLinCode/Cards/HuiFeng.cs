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
using WanJieRuLin.Aspects;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// ★ **点 · 无限流一环** —— 0 费：造成 3 点伤害，**失去 1 点生命**。
/// 升级后 5 点伤害。
/// ★ 单独用很亏（自己掉血只换 3 点伤害）。它的价值在**联动**：
///    打出它会给「接笔」亮灯，接笔因此多产 1 点能量 —— 两张凑起来才开始成立。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class HuiFeng : WanJieRuLinCardModel
{
    public HuiFeng() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Point;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Move),
        ModCardVars.Int("BloodCost", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is { } self)
        {
            await CreatureCmd.Damage(choiceContext, self.Creature,
                DynamicVars.GetIntOrDefault("BloodCost", 1), ValueProp.Move, self.Creature, null, null);
        }
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        await ApplySelf<HuiFengTracePower>(choiceContext, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);    }
}
