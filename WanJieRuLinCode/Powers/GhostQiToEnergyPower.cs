using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 我若为神 —— 能力：**下回合**获得的鬼气转为等量能量。
/// 通过 <see cref="ISecondaryResourceHookListener.AfterSecondaryResourceChanged"/> 监听鬼气获得。
///
/// ★ 修复（2026-09-27，与 EnergyToGhostQiPower 是同一类 bug）：
///   旧写法在 <c>AfterPlayerTurnStart</c> 里把 <c>_armed</c> 置 true 后**永不复位**，
///   于是这个能力一旦上身，**整场战斗**获得的鬼气都会被转成能量 ——
///   而卡面文案明明写的是「**下回合**」。
///
///   后果不只是文案不符：它会永久剥夺玩家积累鬼气的能力，
///   而鬼气正是本角色三条流派的核心资源，等于把卡组废掉一半。
///
///   现在转换满一个回合即自行移除。
/// </summary>
[RegisterPower]
public sealed class GhostQiToEnergyPower : ModPowerTemplate, ISecondaryResourceHookListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>是否处于「下回合」的转换窗口内。</summary>
    private bool _armed;

    /// <summary>本回合窗口是否已经结算过（用来自我移除）。</summary>
    private bool _settled;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || _settled)
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

        // 只作用一个回合：结算一次后关窗，并在回合结束时移除自身。
        _armed = false;
        _settled = true;
        _ = PowerCmd.Remove(this);
        return Task.CompletedTask;
    }
}
