using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 我若为神 —— 能力：下回合获得的鬼气转为等量能量。
/// 通过 ISecondaryResourceHookListener 监听鬼气获得，把增量转成能量。
/// </summary>
[RegisterPower]
public sealed class GhostQiToEnergyPower : ModPowerTemplate, ISecondaryResourceHookListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>是否已进入「下回合」（只在下回合开始后转换）。</summary>
    private bool _armed;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return Task.CompletedTask;
        }

        _armed = true;
        return Task.CompletedTask;
    }

    /// <summary>鬼气增加时，把等量增量转成能量。</summary>
    public Task AfterSecondaryResourceChanged(SecondaryResourceChangeContext context)
    {
        if (!_armed ||
            context.Definition.Id != ModResources.GhostQiId ||
            context.Delta <= 0 ||
            Owner.Player is not { } player)
        {
            return Task.CompletedTask;
        }

        Flash();

        // 该钩子无法 await，转成独立的可等待任务。
        _ = PlayerCmd.GainEnergy(context.Delta, player);
        return Task.CompletedTask;
    }
}
