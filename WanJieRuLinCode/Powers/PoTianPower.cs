using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using WanJieRuLin.Aspects;

namespace WanJieRuLin.Powers;

/// <summary>
/// 泼天 —— ★ **偏激流 · 纯面** 的支撑能力。
///
/// 效果：**墨韵不再削弱「点 / 线」牌**，改为**每满
/// <see cref="WanJieV05Tuning.MoYunPenaltyStep"/> 层，你的「面」牌伤害 +{PerStep}**。
///
/// 设计意图：把墨韵从「双刃」变成「纯增益」，于是「只堆面牌」成为一条
/// 越打越强的路线 —— 这正是「细水长流」的极端形态。
/// 代价是你彻底放弃了点/线，前期会很难受。
/// </summary>
[RegisterPower]
public sealed class PoTianPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("PerStep", 3)
    ];

    /// <summary>墨韵每满一级，面牌额外 +多少伤害。</summary>
    public int PerStep { get; set; } = 3;

    /// <summary>玩家身上是否有「泼天」（供 <see cref="MoYunPower"/> 检查，决定是否跳过削弱）。</summary>
    public static bool Active(Creature? creature)
        => creature is not null
           && creature.GetPowerAmount<PoTianPower>() > 0;

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner || cardSource is null)
        {
            return 0m;
        }

        // 只强化「面」。
        if (!WanJieAspectQuery.IsFace(cardSource))
        {
            return 0m;
        }

        var steps = MoYunPower.Of(Owner) / Math.Max(1, WanJieV05Tuning.MoYunPenaltyStep);
        return steps <= 0 ? 0m : steps * Math.Max(1, PerStep);
    }
}
