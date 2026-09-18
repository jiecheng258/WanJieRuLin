using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 光明预言 —— 能力：每消耗 1 点鬼气，扣除 2 点生命。
/// 扣血可格挡（普通伤害），与「发现无色牌」的正面效果构成代价交换。
/// </summary>
[RegisterPower]
public sealed class GuangMingYuYanPower : ModPowerTemplate, ISecondaryResourceHookListener
{
    /// <summary>每消耗 1 点鬼气扣除的生命。</summary>
    public const int HpLostPerGhostQi = 2;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public Task AfterSecondaryResourceSpent(SecondaryResourceSpendContext context)
    {
        if (context.Definition.Id != ModResources.GhostQiId ||
            context.Amount <= 0 ||
            Owner is not { } creature ||
            Owner.Player is not { } player)
        {
            return Task.CompletedTask;
        }

        Flash();

        // 该钩子无法 await，转成独立的可等待任务。
        var ctx = new HookPlayerChoiceContext(this, player, player.NetId, GameActionType.Combat);
        _ = CreatureCmd.Damage(
            ctx, creature, context.Amount * HpLostPerGhostQi,
            ValueProp.Move, player.Creature, null, null);

        return Task.CompletedTask;
    }
}
