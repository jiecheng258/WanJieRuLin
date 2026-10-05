using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 笔锋 —— 「点」牌的核心产出。
///
/// 效果：**本回合内，你打出的下一张牌费用 −{Amount}**（扣到 0 为止），打出后立即清空。
///
/// 设计意图：这是「点 → 面」的桥梁 —— 用便宜的点牌把昂贵的面牌提前抬上场。
///
/// 实现说明：
/// 真正的减费在卡牌侧（<c>WanJieRuLinCardModel</c> 实现
/// <c>ICardEnergyCostContributor.ModifyEnergyCost</c>），读出本方玩家的笔锋层数后扣减。
/// 本能力只负责「持有层数」与「打出后清空」。
/// 继承 <see cref="WanJieTurnScopedPower"/> 保证回合结束自动撤掉。
/// </summary>
[RegisterPower]
public sealed class BiFengPower : WanJieTurnScopedPower
{
    public override PowerType Type => PowerType.Buff;

    /// <summary>可叠加：每层让下一张牌便宜 1 费。</summary>
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    /// <summary>本回合是否已经用掉笔锋（用掉后不再减费）。</summary>
    private bool _consumed;

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (Owner is null || _consumed)
        {
            return;
        }

        // ★ v0.6 修复：**0 费牌不消耗笔锋**。
        //
        // 原因：点牌大多是 0 费，而点牌自己就是笔锋的产出方。
        // 如果「打任何牌都清空」，那么「起笔(0费) → 再打一张 0 费点牌」
        // 会把笔锋白白吃掉（减费 0 效果），点牌的核心产出被点牌自己浪费掉了。
        // 改成只看「这张牌实际花了几费」，0 费牌直接跳过。
        if (cardPlay.Card is null || cardPlay.Card.EnergyCost.GetAmountToSpend() <= 0)
        {
            return;
        }

        // 笔锋只服务「下一张牌」—— 立刻把其余手牌的费用恢复原样。
        if (Owner?.Player is { } player)
        {
            Cards.WanJieRuLinCardModel.ClearBiFengFromHand(player, Math.Max(0, Amount));
        }

        _consumed = true;
        await PowerCmd.Remove(this);
    }

    /// <summary>
    /// ★ 笔锋的减费就在这里实现。
    ///
    /// 为什么放在 Power 而不是卡牌：费用修改的钩子是
    /// <c>PowerModel.TryModifyEnergyCostInCombat(card, originalCost, out modifiedCost)</c>,
    /// 原版的 CorruptionPower / FreeAttackPower / TangledPower 都是重写它。
    /// 卡牌侧那套 ICardEnergyCostContributor 属于 RitsuLib 的**能力（Capability）系统**，
    /// 必须在模型上显式 AddCapability 才会被调用 —— 只「实现接口」不生效
    /// （这就是之前笔锋没有减费的根因）。
    ///
    /// 返回 true 表示「我改了这个费用」，框架才会采用 modifiedCost。
    /// </summary>
    /// <summary>取某生物当前的笔锋层数（没有该能力时为 0）。</summary>
    public static int Of(Creature? creature)
    {
        if (creature is null)
        {
            return 0;
        }

        var p = creature.GetPowerAmount<BiFengPower>();
        return p > 0 ? p : 0;
    }
}
