using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
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
/// 浓墨重彩 —— 能力：每当你**获得**鬼气，获得等量的格挡（每回合最多 <see cref="MaxBlockPerTurn"/> 点）。
///
/// 「蓄鬼」流派的收尾件：把「攒气」这个动作直接转成防御，
/// 于是玩家可以放心地把鬼气堆到 8 以上吃「墨浓」的伤害惩罚 —— 因为防守端补回来了。
/// </summary>
[RegisterPower]
public sealed class RichInkPower : ModPowerTemplate, ISecondaryResourceHookListener
{
    /// <summary>每回合最多转化的格挡（由卡牌在施加时写入）。</summary>
    public int MaxBlockPerTurn { get; set; } = 12;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>上一次观察到的鬼气。-1 表示尚未建立基线。</summary>
    private int _lastKnown = -1;

    /// <summary>本回合已转化的格挡。</summary>
    private int _blockThisTurn;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner is { } owner && player.Creature == owner)
        {
            _blockThisTurn = 0;
            _lastKnown = GhostQi.Get(player);
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
        _lastKnown = current;

        if (delta <= 0)
        {
            return Task.CompletedTask;
        }

        var room = MaxBlockPerTurn - _blockThisTurn;
        if (room <= 0)
        {
            return Task.CompletedTask;
        }

        var gain = Math.Min(delta, room);
        _blockThisTurn += gain;
        Flash();

        var combatState = Owner.CombatState;
        if (combatState is null)
        {
            return Task.CompletedTask;
        }

        var ctx = new HookPlayerChoiceContext(this, player.NetId, combatState, GameActionType.Combat);
        _ = GrantAsync(ctx, gain);
        return Task.CompletedTask;
    }

    private async Task GrantAsync(PlayerChoiceContext ctx, int amount)
    {
        if (Owner is { } owner)
        {
            await CreatureCmd.GainBlock(owner, amount, ValueProp.Move, null);
        }
    }
}
