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
/// 效果：
/// - 战斗开始时获得 1 点鬼气（由鬼气资源自身的「每场战斗起始值」提供）。
/// - 每回合开始时获得 1 点鬼气。
///
/// 鬼气是本角色的第二资源（星辉式：一场战斗内跨回合保留，进入下一场战斗时重置），
/// 用于驱动「绘」系的 X 费牌与各类鬼气消耗牌。
/// </summary>
[RegisterRelic(typeof(WanJieRuLinRelicPool))]
[RegisterCharacterStarterRelic(typeof(WanJieRuLinCharacter))]
public sealed class GuiMo : WanJieRuLinRelic
{
    /// <summary>每回合开始时获得的鬼气。</summary>
    public const int TurnStartGhostQi = 1;

    public override RelicRarity Rarity => RelicRarity.Starter;

    // 战斗开始：基础 1 点由 ModResources.GhostQiCombatStartAmount 提供。
    // 这里只做「重置到本场战斗起点」，避免上一场战斗的残留鬼气叠加过来
    // （鬼气是星辉式资源：战斗内跨回合保留，换战斗则归位）。
    //
    // 说明：鬼气资源本身是 PersistencePolicy.Combat，正常情况下框架在换战斗时
    // 就会回到 defaultAmount；这里是显式兜底，顺序上「重置成 1」不会和框架冲突
    // （即使框架在本钩子之后再重置一次，结果仍然是 1）。
    public override Task BeforeCombatStart() => ResetGhostQiToCombatStart();

    // 每回合开始：持续供给，构成鬼气经济的基础。
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
        {
            return;
        }

        Flash();
        await GainGhostQi(TurnStartGhostQi);
    }
}
