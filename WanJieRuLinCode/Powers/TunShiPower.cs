using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 吞噬 —— 能力：每获得 1 点鬼气，抽 1 张牌。
///
/// 通过 <see cref="ISecondaryResourceHookListener.AfterSecondaryResourceChanged"/> 监听鬼气变化。
/// 每次鬼气增加，按增量抽等量张牌。
/// </summary>
[RegisterPower]
public sealed class TunShiPower : ModPowerTemplate, ISecondaryResourceHookListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>上一次观察到的鬼气值，用于计算增量。-1 表示尚未初始化。</summary>
    private int _lastKnown = -1;

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
        _lastKnown = current;

        if (delta <= 0)
        {
            return Task.CompletedTask;
        }

        Flash();

        // 在这里无法 await，交给一个独立的可等待任务执行抽牌。
        var combatState = Owner.CombatState;
        if (combatState is null)
        {
            return Task.CompletedTask;
        }

        var ctx = new HookPlayerChoiceContext(this, player.NetId, combatState, GameActionType.Combat);
        _ = DrawAsync(ctx, player, delta);
        return Task.CompletedTask;
    }

    private static async Task DrawAsync(PlayerChoiceContext ctx, Player player, int count)
    {
        for (var i = 0; i < count; i++)
        {
            await CardPileCmd.Draw(ctx, 1, player);
        }
    }
}
