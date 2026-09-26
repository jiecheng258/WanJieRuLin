using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Scaffolding.Content;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Relics;

/// <summary>
/// 万界如林遗物基类：统一图标路径，并提供鬼气读写快捷方法。
/// </summary>
public abstract class WanJieRuLinRelic : ModRelicTemplate
{
    /// <summary>本场战斗是否已经挂过「墨之相」。</summary>
    private bool _inkPhaseApplied;

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

    /// <summary>
    /// 进入新战斗时调用，重置「墨之相」的施加标记。
    /// </summary>
    protected void ResetInkPhaseFlag() => _inkPhaseApplied = false;

    /// <summary>
    /// 确保「墨之相」（<see cref="InkPhasePower"/>）已经挂在自己身上。
    ///
    /// 为什么不在 <see cref="BeforeCombatStart"/> 里挂：那个钩子**没有
    /// <see cref="PlayerChoiceContext"/>**，而 <c>PowerCmd.Apply</c> 必须要一个上下文。
    /// 所以放到第一个回合的 <c>AfterPlayerTurnStart</c>（那时上下文齐全，
    /// 且早于玩家出牌，惩罚/增益当回合就能生效）。
    ///
    /// 为什么用标记而不是「查有没有这个能力」：起始遗物（鬼墨 / 金丝鬼墨）互斥，
    /// 正常情况下只会有一个在生效；标记只是防止同一场战斗里被重复施加。
    /// </summary>
    protected async Task EnsureInkPhase(PlayerChoiceContext ctx, Player player)
    {
        if (_inkPhaseApplied)
        {
            return;
        }

        _inkPhaseApplied = true;

        if (Owner != player)
        {
            return;
        }

        await PowerCmd.Apply<InkPhasePower>(ctx, player.Creature, 1m, player.Creature, null);
    }
}
