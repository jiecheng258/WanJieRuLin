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
/// 泼天 —— ★ **纯面流**的支撑能力（v0.7 版）。
///
/// 效果：你的「面」牌伤害额外 +{PerStep}。
///
/// 设计意图：让「只堆面牌」成为一条越打越强的路线。
/// 面牌本身数值就高，这个能力再给一层常驻加成。
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
        ModCardVars.Int("PerStep", 6)
    ];

    /// <summary>每张面牌额外 +多少伤害。</summary>
    public int PerStep { get; set; } = 6;

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

        return WanJieAspectQuery.IsFace(cardSource) ? Math.Max(1, PerStep) : 0m;
    }
}
