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
/// 化神 —— 能力：每消耗 1 点鬼气，抽 1 张牌并获得 1 点能量。
///
/// 监听 <see cref="ISecondaryResourceHookListener.AfterSecondaryResourceSpent"/>，
/// 把鬼气的消耗转化为手牌与能量的循环。
/// </summary>
[RegisterPower]
public sealed class HuaShenPower : ModPowerTemplate, ISecondaryResourceHookListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public Task AfterSecondaryResourceSpent(SecondaryResourceSpendContext context)
    {
        var player = Owner.Player;
        if (player is null || context.Definition.Id != ModResources.GhostQiId)
        {
            return Task.CompletedTask;
        }

        var spent = (int)context.Amount;
        if (spent <= 0)
        {
            return Task.CompletedTask;
        }

        Flash();

        var combatState = Owner.CombatState;
        if (combatState is null)
        {
            return Task.CompletedTask;
        }

        var ctx = new HookPlayerChoiceContext(this, player.NetId, combatState, GameActionType.Combat);
        _ = GrantAsync(ctx, player, spent);
        return Task.CompletedTask;
    }

    private static async Task GrantAsync(PlayerChoiceContext ctx, Player player, int times)
    {
        for (var i = 0; i < times; i++)
        {
            await PlayerCmd.GainEnergy(1, player);
            await CardPileCmd.Draw(ctx, 1, player);
        }
    }
}
