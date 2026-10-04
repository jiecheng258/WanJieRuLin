using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 力道 —— 「线」牌的核心产出。
///
/// 效果：**本回合内，你打出的牌 伤害与格挡 +{Amount}**。回合结束自动消失。
///
/// 设计意图：这是「线 → 面」的放大器 —— 面牌本身数值就高，再被力道抬一档。
/// 与笔锋（削费）互补：笔锋解决「打不起」，力道解决「打不痛」。
/// </summary>
[RegisterPower]
public sealed class LiDaoPower : WanJieTurnScopedPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    /// <summary>取某生物当前的力道层数。</summary>
    public static int Of(Creature? creature)
        => creature is null ? 0 : Math.Max(0, creature.GetPowerAmount<LiDaoPower>());

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        // 只加自己造成的、来自卡牌的伤害。
        if (dealer != Owner || cardSource is null)
        {
            return 0m;
        }

        return Amount;
    }

    public override decimal ModifyBlockAdditive(
        Creature? target,
        decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner || cardSource is null)
        {
            return 0m;
        }

        return Amount;
    }
}
