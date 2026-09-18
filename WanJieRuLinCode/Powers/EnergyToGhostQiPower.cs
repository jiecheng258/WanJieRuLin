using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 我若为鬼 —— 能力：下回合获得的能量转为等量鬼气值（能量本身不再获得）。
/// 参照原版 NoEnergyGainPower 的实现方式：
/// ModifyEnergyGain 把增量吞掉并记账，AfterModifyingEnergyGain 再把账折算成鬼气。
/// </summary>
[RegisterPower]
public sealed class EnergyToGhostQiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>本轮被拦截、待折算成鬼气的能量。</summary>
    private int _pending;

    /// <summary>是否已进入「下回合」（只在下回合开始后拦截）。</summary>
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

    /// <summary>吞掉能量增量，返回 0。</summary>
    public override decimal ModifyEnergyGain(Player player, decimal amount)
    {
        if (!_armed || player.Creature != Owner || amount <= 0m)
        {
            return amount;
        }

        _pending += (int)amount;
        return 0m;
    }

    /// <summary>把这一批被吞掉的能量一次性折算成鬼气。</summary>
    public override async Task AfterModifyingEnergyGain()
    {
        if (!_armed || _pending <= 0 || Owner.Player is not { } player)
        {
            return;
        }

        var toConvert = _pending;
        _pending = 0;

        Flash();
        await GhostQi.Gain(player, toConvert);
    }
}
