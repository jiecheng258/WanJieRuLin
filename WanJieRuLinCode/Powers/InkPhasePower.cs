using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 墨之相 —— 本角色的**固有机制**，由起始遗物（鬼墨 / 金丝鬼墨）在每场战斗的第一个
/// 回合开始时挂上，整场战斗常驻。
///
/// 设计意图（对齐角色大纲）：
/// 鬼气不是「多多益善」的资源 —— 墨太浓则笔锋滞涩，墨太淡则走笔如飞。
/// 于是把鬼气划分成三个相位，让「攒鬼气」与「清鬼气」成为两条真正互斥的路线：
///
/// | 相位 | 鬼气 | 效果 |
/// |---|---|---|
/// | **墨淡** | ≤ 2 | 每回合开始多抽 1 张牌（低鬼气的过牌补偿） |
/// | **墨匀** | 3 – 7 | 无增减（均衡流的 combo 区间，由「匀墨」类能力奖励） |
/// | **墨浓** | ≥ 8 | 你造成的攻击伤害 −2 |
/// | **墨极浓** | ≥ 14 | 你造成的攻击伤害 −5 |
///
/// 高鬼气的代价刻意**只削攻击伤害**，不削格挡：
/// 玩家仍然可以用高鬼气去开「绘」「墨染江山」这类爆发，但代价是普攻变钝，
/// 于是「高鬼气爆发」与「低鬼气铺场」各有取舍，而不是无脑攒气。
///
/// ⚠️ 实现要点：
/// 1. 这是**整场常驻**的能力，所以继承 <see cref="ModPowerTemplate"/> 而**不是**
///    <see cref="WanJieTurnScopedPower"/>（后者会在回合结束时把自己移除）。
/// 2. <see cref="PowerStackType.Single"/> —— 不可叠加、不显示层数，
///    与「鬼域」「吞噬」等常驻能力保持一致。
/// 3. 文案里的 {ThinMax} / {DenseMin} 等占位符必须在 <see cref="CanonicalVars"/>
///    里声明，否则悬浮提示渲染失败并在日志里刷
///    "No source extension could handle the selector named '...'"。
/// </summary>
[RegisterPower]
public sealed class InkPhasePower : ModPowerTemplate
{
    /// <summary>墨淡的上限：鬼气不高于此值即为「墨淡」。</summary>
    public const int ThinMax = 2;

    /// <summary>墨浓的起点：鬼气不低于此值即为「墨浓」。</summary>
    public const int DenseMin = 8;

    /// <summary>墨极浓的起点。</summary>
    public const int DeepMin = 14;

    /// <summary>墨浓的攻击伤害惩罚。</summary>
    public const int DensePenalty = 2;

    /// <summary>墨极浓的攻击伤害惩罚。</summary>
    public const int DeepPenalty = 5;

    /// <summary>墨淡时每回合额外抽的牌数。</summary>
    public const int ThinDraw = 1;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("ThinMax", ThinMax),
        ModCardVars.Int("DenseMin", DenseMin),
        ModCardVars.Int("DeepMin", DeepMin),
        ModCardVars.Int("DensePenalty", DensePenalty),
        ModCardVars.Int("DeepPenalty", DeepPenalty)
    ];

    /// <summary>
    /// 墨浓 / 墨极浓：下调自己造成的**攻击**伤害。
    ///
    /// 只对 <c>props.IsPoweredAttack()</c> 生效 —— 反伤、持续伤害、能力造成的伤害不受影响，
    /// 这样「鬼影森森」「光明预言」这类非攻击来源不会被误伤。
    /// </summary>
    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner || Owner.Player is not { } player)
        {
            return 0m;
        }

        if (!props.IsPoweredAttack() || amount <= 0m)
        {
            return 0m;
        }

        var qi = GhostQi.Get(player);
        if (qi >= DeepMin)
        {
            return -DeepPenalty;
        }

        if (qi >= DenseMin)
        {
            return -DensePenalty;
        }

        return 0m;
    }

    /// <summary>
    /// 墨淡：自己回合开始时多抽 1 张牌。
    ///
    /// 这既是「低鬼气流派」的引擎（清完鬼气就能滚雪球），
    /// 也是高鬼气的**机会成本** —— 气攒着不花，就一直在丢抽牌。
    /// </summary>
    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || player.Creature.IsDead)
        {
            return;
        }

        if (GhostQi.Get(player) > ThinMax)
        {
            return;
        }

        Flash();
        await CardPileCmd.Draw(choiceContext, ThinDraw, player);
    }
}
