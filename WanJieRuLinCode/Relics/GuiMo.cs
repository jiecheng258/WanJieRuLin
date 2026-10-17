using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Relics;

/// <summary>
/// 鬼墨（初始遗物）
///
/// 效果：每场战斗开始时获得 1 点鬼气。
///
/// 鬼气是本角色的第二资源（星辉式：一场战斗内跨回合保留，进入下一场战斗时重置），
/// 专供「作祟」等 X 鬼气牌消耗。
///
/// 用户大纲定稿：鬼墨 = 每场战斗开始 +1 鬼气（不是每回合）。
/// </summary>
[RegisterRelic(typeof(WanJieRuLinRelicPool))]
[RegisterCharacterStarterRelic(typeof(WanJieRuLinCharacter))]
public sealed class GuiMo : WanJieRuLinRelic
{
    /// <summary>每场战斗开始时获得的鬼气。</summary>
    public const int CombatStartGhostQi = 1;

    /// <summary>本场战斗的开局爆发是否已经发放过。</summary>
    private bool _combatStartApplied;

    public override RelicRarity Rarity => RelicRarity.Starter;

    // 进入新战斗时只重置标记，不直接改数值 —— 见 AfterPlayerTurnStart 的说明。
    public override Task BeforeCombatStart()
    {
        _combatStartApplied = false;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
        {
            return;
        }

        // 开局爆发放在「本场战斗的第一个回合开始」而不是 BeforeCombatStart：
        // 鬼气资源自身是 PersistencePolicy.Combat，框架可能在本遗物的
        // BeforeCombatStart 之后才把数值重置到 defaultAmount（=1）。
        // 如果在那之前改数值，+1 会被框架的重置覆盖掉。
        // 放到第一次回合开始，一定晚于所有战斗初始化，顺序就无关了。
        if (!_combatStartApplied)
        {
            _combatStartApplied = true;
            await ResetGhostQiToCombatStart();

            // 挂上常驻探测器：每打一张牌，本回合打牌计数 +1（供条件牌探测）。
            await MegaCrit.Sts2.Core.Commands.PowerCmd.Apply<
                WanJieRuLin.Powers.WanJieRuLinCombatTrackerPower>(
                choiceContext, player.Creature, 1m, player.Creature, null);

            if (CombatStartGhostQi > 0)
            {
                Flash();
                await GainGhostQi(CombatStartGhostQi);
            }
        }
    }
}
