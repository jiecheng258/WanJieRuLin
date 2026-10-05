using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Characters;

namespace WanJieRuLin.Powers;

/// <summary>
/// 「本回合的临时属性」基类 —— 力量 / 敏捷 共用。
///
/// - 继承 <see cref="WanJieTurnScopedPower"/>：回合结束自动移除（所以叫「临时」）
/// - 只加自己造成的伤害（力量）与自己获得的格挡（敏捷）
/// </summary>
public abstract class WanJieTempStatPower : WanJieTurnScopedPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>子类给出自己要加成的那一项（Strength / Dexterity）。</summary>
    protected abstract DynamicVar StatVar { get; }

    protected override IEnumerable<DynamicVar> CanonicalVars => [StatVar];

    /// <summary>本能力的加成量（= 层数 × 每层数值）。</summary>
    protected decimal Bonus => Amount;

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
        => StatVar.Name == "Strength" && dealer == Owner && cardSource is not null
            ? Bonus
            : 0m;

    public override decimal ModifyBlockAdditive(
        Creature? target,
        decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay)
        => StatVar.Name == "Dexterity" && target == Owner && cardSource is not null
            ? Bonus
            : 0m;
}
