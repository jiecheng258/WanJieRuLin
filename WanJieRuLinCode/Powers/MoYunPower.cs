using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using WanJieRuLin.Aspects;

namespace WanJieRuLin.Powers;

/// <summary>
/// 墨韵 —— 「面」牌的核心产出，也是**整条设计的分水岭**。
///
/// 效果：**跨回合累积，不会清空**。但每满 <see cref="WanJieV05Tuning.MoYunPenaltyStep"/>
/// 层，你的「点」与「线」牌效果 −1（最低降到 1）。
///
/// ★ 这就是「细水长流 vs 闪电战」的来源：
///   - 堆墨韵 → 面牌收益暴涨，但点/线变钝 → **细水长流**（后期强）
///   - 不堆墨韵 → 点/线保持锋利，快速循环 → **闪电战**（后期没有成长）
///   两者天然矛盾 —— 不可能同时拥有高墨韵和锋利的点线。
///
/// 注意：这是**战斗内**累积（不像笔锋/力道那样回合结束消失），
/// 所以不能继承 WanJieTurnScopedPower。
/// </summary>
[RegisterPower]
public sealed class MoYunPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("PenaltyStep", WanJieV05Tuning.MoYunPenaltyStep)
    ];

    /// <summary>取某生物当前的墨韵层数。</summary>
    public static int Of(Creature? creature)
        => creature is null ? 0 : Math.Max(0, creature.GetPowerAmount<MoYunPower>());

    /// <summary>增减墨韵（amount 可为负）。</summary>
    public static async Task Gain(
        MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext ctx,
        MegaCrit.Sts2.Core.Entities.Creatures.Creature creature,
        int amount)
    {
        if (amount != 0)
        {
            await MegaCrit.Sts2.Core.Commands.PowerCmd.Apply<MoYunPower>(
                ctx, creature, amount, creature, null);
        }
    }

    /// <summary>当前的「点/线削弱值」= 墨韵 / 每级步长。</summary>
    public static int PenaltyOf(Creature? creature)
        => Of(creature) / Math.Max(1, WanJieV05Tuning.MoYunPenaltyStep);

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

        // 只削弱「点 / 线」，且不把伤害压到负数。
        if (!WanJieAspectQuery.IsPointOrLine(cardSource))
        {
            return 0m;
        }

        // ★ 偏激流·纯面「泼天」：墨韵不再削弱点/线（改由 PoTianPower 强化面牌）。
        if (PoTianPower.Active(Owner))
        {
            return 0m;
        }

        var penalty = PenaltyOf(Owner);
        if (penalty <= 0)
        {
            return 0m;
        }

        var room = (int)amount - WanJieV05Tuning.MoYunPenaltyFloor;
        return -Math.Min(penalty, Math.Max(0, room));
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

        if (!WanJieAspectQuery.IsPointOrLine(cardSource))
        {
            return 0m;
        }

        // ★ 偏激流·纯面「泼天」：墨韵不再削弱点/线（改由 PoTianPower 强化面牌）。
        if (PoTianPower.Active(Owner))
        {
            return 0m;
        }

        var penalty = PenaltyOf(Owner);
        if (penalty <= 0)
        {
            return 0m;
        }

        var room = (int)block - WanJieV05Tuning.MoYunPenaltyFloor;
        return -Math.Min(penalty, Math.Max(0, room));
    }
}
