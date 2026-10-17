using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Relics;

/// <summary>
/// 金丝鬼墨（鬼墨的先古升级形态）
///
/// 效果：每场战斗开始时获得 2 点鬼气。
///
/// 用户大纲定稿：金丝鬼墨 = 每场战斗开始 +2 鬼气（不是每回合）。
/// 与鬼墨相比，开局爆发从 1 提升到 2，让作祟能更早启动。
/// </summary>
[RegisterRelic(typeof(WanJieRuLinRelicPool))]
public sealed class JinSiGuiMo : WanJieRuLinRelic
{
    /// <summary>每场战斗开始时获得的鬼气。</summary>
    public const int CombatStartGhostQi = 2;

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

        // 同 GuiMo：开局爆发放第一次回合开始，避开框架重置鬼气的时序。
        if (!_combatStartApplied)
        {
            _combatStartApplied = true;
            await ResetGhostQiToCombatStart();

            if (CombatStartGhostQi > 0)
            {
                Flash();
                await GainGhostQi(CombatStartGhostQi);
            }
        }
    }
}
