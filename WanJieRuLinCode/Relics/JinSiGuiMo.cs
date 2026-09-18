using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Relics;

/// <summary>
/// 金丝鬼墨（鬼墨的升级形态）
///
/// 效果：
/// - 战斗开始时获得 3 点鬼气（资源默认 1 点 + 额外 2 点）。
/// - 每回合开始时获得 1 点鬼气。
///
/// 与鬼墨共享「每回合 +1」的持续供给，但开局爆发从 1 提升到 3，
/// 让高费鬼气牌（如墨染江山）能更早启动。
/// </summary>
[RegisterRelic(typeof(WanJieRuLinRelicPool))]
public sealed class JinSiGuiMo : WanJieRuLinRelic
{
    /// <summary>战斗开始时在本场起始值（1）之上额外获得的鬼气。</summary>
    public const int ExtraCombatStartGhostQi = 2;

    /// <summary>每回合开始时获得的鬼气。</summary>
    public const int TurnStartGhostQi = 1;

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
        // 如果在那之前改数值，+2 会被框架的重置覆盖掉。
        // 放到第一次回合开始，一定晚于所有战斗初始化，顺序就无关了。
        if (!_combatStartApplied)
        {
            _combatStartApplied = true;
            await ResetGhostQiToCombatStart();

            if (ExtraCombatStartGhostQi > 0)
            {
                Flash();
                await GainGhostQi(ExtraCombatStartGhostQi);
            }
        }

        Flash();
        await GainGhostQi(TurnStartGhostQi);
    }
}
