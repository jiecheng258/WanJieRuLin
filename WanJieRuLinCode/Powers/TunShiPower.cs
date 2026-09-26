using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 吞噬 —— 能力：每回合最多触发 2 次，每次获得鬼气时抽 1 张牌。
///
/// ★ 反无限（R5）—— 旧写法是**最直接的无限引擎**：
///   「**每获得 1 点鬼气**就抽 1 张牌」，**次数完全无上限**。
///   只要配一张产鬼气的牌（尤其当时 0 费的「层层侵蚀」），
///   产气 → 抽牌 → 抽回产气牌 → 再产气 …… 当场就是无限抽牌。
///
///   收紧：**每回合限 2 次**，且每次只抽 1 张（不再按 delta 放大）。
///
/// ⚠️ 实现上两个必须注意的点（都是踩过的坑）：
///   1. **首次回调只记基线、不触发** —— 否则能力刚上身就把玩家已有的鬼气
///      一次性兑换成手牌。
///   2. **先推进基线、再判断触发额度** —— 顺序反了的话，额度用尽后基线会卡住不动，
///      之后所有增量都被吞掉（表现为「只有前几次生效，后面完全不响应」）。
/// </summary>
[RegisterPower]
public sealed class TunShiPower : ModPowerTemplate, ISecondaryResourceHookListener
{
    /// <summary>每回合最多触发次数。</summary>
    private const int MaxTriggersPerTurn = 2;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>上一次观察到的鬼气值，用于计算增量。-1 表示尚未初始化。</summary>
    private int _lastKnown = -1;

    /// <summary>本回合已触发次数。</summary>
    private int _triggersThisTurn;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner)
        {
            _triggersThisTurn = 0;
            _lastKnown = GhostQi.Get(player);   // 每回合重建基线，避免跨回合误触发
        }

        return Task.CompletedTask;
    }

    public Task AfterSecondaryResourceChanged(SecondaryResourceChangeContext context)
    {
        var player = Owner.Player;
        if (player is null || context.Definition.Id != ModResources.GhostQiId)
        {
            return Task.CompletedTask;
        }

        var current = GhostQi.Get(player);

        // 首次观察只记基线，不触发抽牌（避免能力刚生效就把既有鬼气全换成牌）。
        if (_lastKnown < 0)
        {
            _lastKnown = current;
            return Task.CompletedTask;
        }

        var delta = current - _lastKnown;

        // ★ 先推进基线，再判断额度 —— 顺序反了会让基线卡住、后续增量被吞。
        _lastKnown = current;

        if (delta <= 0 || _triggersThisTurn >= MaxTriggersPerTurn)
        {
            return Task.CompletedTask;
        }

        _triggersThisTurn++;
        Flash();

        // 在这里无法 await，交给一个独立的可等待任务执行抽牌。
        var combatState = Owner.CombatState;
        if (combatState is null)
        {
            return Task.CompletedTask;
        }

        var ctx = new HookPlayerChoiceContext(this, player.NetId, combatState, GameActionType.Combat);
        _ = CardPileCmd.Draw(ctx, 1, player);
        return Task.CompletedTask;
    }
}
