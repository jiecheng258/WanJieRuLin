using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Relics;

/// <summary>
/// 万界如林遗物基类：统一图标路径，并提供鬼气读写快捷方法。
/// </summary>
public abstract class WanJieRuLinRelic : ModRelicTemplate
{
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    /// <summary>给遗物拥有者加鬼气。</summary>
    protected Task GainGhostQi(int amount) =>
        Owner is { } player ? GhostQi.Gain(player, amount) : Task.CompletedTask;

    /// <summary>读取遗物拥有者当前的鬼气。</summary>
    protected int CurrentGhostQi => Owner is { } player ? GhostQi.Get(player) : 0;

    /// <summary>
    /// 把鬼气重置为「每场战斗起始值」。
    ///
    /// 鬼气是星辉式资源：一场战斗内跨回合保留，但不跨战斗叠加。
    /// 进入新战斗时调用本方法，确保从固定起点（1 点）重新开始。
    /// </summary>
    protected Task ResetGhostQiToCombatStart() =>
        Owner is { } player
            ? GhostQi.Set(player, ModResources.GhostQiCombatStartAmount)
            : Task.CompletedTask;
}
