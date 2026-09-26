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
/// 化神 —— 能力：每回合最多触发 2 次，每次消耗鬼气时抽 1 张牌并获得 1 点能量。
///
/// ★ 反无限（R5）—— 旧写法是**无限引擎**：
///   「每消耗 1 点鬼气 → 抽 1 张牌 **+ 1 点能量**」，且**次数无上限、按消耗量放大**。
///   只要配一张产鬼气的牌就是净赚循环：
///   产气牌（1 费→2 鬼气）→ 消耗这 2 点 → 拿回 2 能量 + 2 张牌 ⇒ 1 费换 2 能量。
///
///   收紧：**每回合限 2 次**，且每次固定 1 能量 1 张牌（不再按消耗量放大）。
/// </summary>
[RegisterPower]
public sealed class HuaShenPower : ModPowerTemplate, ISecondaryResourceHookListener
{
    /// <summary>每回合最多触发次数。</summary>
    private const int MaxTriggersPerTurn = 2;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>本回合已触发次数。</summary>
    private int _triggersThisTurn;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner)
        {
            _triggersThisTurn = 0;
        }

        return Task.CompletedTask;
    }

    public Task AfterSecondaryResourceSpent(SecondaryResourceSpendContext context)
    {
        var player = Owner.Player;
        if (player is null ||
            context.Definition.Id != ModResources.GhostQiId ||
            context.Amount <= 0 ||
            _triggersThisTurn >= MaxTriggersPerTurn)
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
        _ = GrantAsync(ctx, player);
        return Task.CompletedTask;
    }

    private static async Task GrantAsync(PlayerChoiceContext ctx, Player player)
    {
        await PlayerCmd.GainEnergy(1, player);
        await CardPileCmd.Draw(ctx, 1, player);
    }
}
