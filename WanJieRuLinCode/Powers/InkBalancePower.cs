using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 浓淡由心 —— 能力：鬼气**升高**时获得格挡，鬼气**降低**时抽牌。
/// 每回合最多触发 <see cref="MaxTriggersPerTurn"/> 次。
///
/// 「墨匀」流派的收益放大器：让「调鬼气」这个动作本身产生价值，
/// 与鬼气循环 / 匀墨 / 归一 搭配时每回合都能白拿资源。
///
/// 实现沿用 <see cref="TunShiPower"/> 的增量记账法：
/// <c>AfterSecondaryResourceChanged</c> 里对比上一次的观测值求 delta。
/// </summary>
[RegisterPower]
public sealed class InkBalancePower : ModPowerTemplate, ISecondaryResourceHookListener
{
    /// <summary>每回合最多触发的次数（由卡牌在施加时写入）。</summary>
    public int MaxTriggersPerTurn { get; set; } = 3;

    /// <summary>每触发一次获得的格挡。</summary>
    public const int BlockPerTrigger = 2;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>上一次观察到的鬼气。-1 表示尚未建立基线。</summary>
    private int _lastKnown = -1;

    /// <summary>本回合已触发次数。</summary>
    private int _triggersThisTurn;

    /// <summary>本回合是否还有触发额度。</summary>
    private bool HasBudget => _triggersThisTurn < Math.Max(1, MaxTriggersPerTurn);

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (Owner is { } owner && player.Creature == owner)
        {
            _triggersThisTurn = 0;
            _lastKnown = GhostQi.Get(player);      // 每回合重建基线，避免跨回合误触发
        }

        return Task.CompletedTask;
    }

    public Task AfterSecondaryResourceChanged(SecondaryResourceChangeContext context)
    {
        if (Owner.Player is not { } player ||
            context.Definition.Id != ModResources.GhostQiId)
        {
            return Task.CompletedTask;
        }

        var current = GhostQi.Get(player);
        if (_lastKnown < 0)
        {
            _lastKnown = current;
            return Task.CompletedTask;
        }

        var delta = current - _lastKnown;
        if (delta == 0)
        {
            return Task.CompletedTask;
        }

        // ★ 先推进基线，再判断额度 —— 否则额度用尽后基线会卡住、后续增量被吞。
        _lastKnown = current;

        if (!HasBudget)
        {
            return Task.CompletedTask;
        }

        _triggersThisTurn++;
        Flash();

        var combatState = Owner.CombatState;
        if (combatState is null)
        {
            return Task.CompletedTask;
        }

        var ctx = new HookPlayerChoiceContext(this, player.NetId, combatState, GameActionType.Combat);
        _ = (delta > 0 ? GainBlockAsync(ctx) : DrawAsync(ctx, player));
        return Task.CompletedTask;
    }

    private async Task GainBlockAsync(PlayerChoiceContext ctx)
    {
        if (Owner is { } owner)
        {
            await CreatureCmd.GainBlock(owner, BlockPerTrigger, ValueProp.Move, null);
        }
    }

    private static async Task DrawAsync(PlayerChoiceContext ctx, MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        await CardPileCmd.Draw(ctx, 1, player);
    }
}
