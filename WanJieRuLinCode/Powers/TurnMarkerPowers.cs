using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

// ============================================================================
// 回合标记能力三件套（v0.9 作祟联动用）
//
// 大量牌需要「本回合已打过作祟」「本回合已获得鬼气」「本回合已打了 N 张牌」
// 这类条件。用三个「回合限定」标记能力记录，回合结束自动清除。
//
// 三个都是继承 WanJieTurnScopedPower 的纯标记（StackType.Counter），
// 本身没有效果钩子，只作为 GetPowerAmount 探测的存在性 + 计数。
// ============================================================================

/// <summary>标记：本回合是否已打出过「作祟」。每打一次作祟 +1。</summary>
[RegisterPower]
public sealed class ZuoSuiPlayedThisTurnPower : WanJieTurnScopedPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}

/// <summary>标记：本回合是否已获得过鬼气。每获得一次鬼气 +1。</summary>
[RegisterPower]
public sealed class GhostQiGainedThisTurnPower : WanJieTurnScopedPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}

/// <summary>计数：本回合已打出的牌数（每打一张牌 +1，作祟也计入）。</summary>
[RegisterPower]
public sealed class CardsPlayedThisTurnPower : WanJieTurnScopedPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}

// ============================================================================
// 战斗探测器（v0.9）—— 常驻、每场战斗开始时自动挂到玩家身上。
//
// 负责：每打出一张牌就把 CardsPlayedThisTurnPower 计数 +1，
// 供「本回合已打 N 张牌」这类条件牌探测。
//
// 它继承 ModPowerTemplate（常驻），由起始遗物「鬼墨」在战斗开始时施加；
// 若玩家开局有「鬼墨」（必有），则本探测器一定在场。
// ============================================================================

/// <summary>
/// 常驻探测器：每打出一张牌，本回合打牌计数 +1。
/// </summary>
[RegisterPower]
public sealed class WanJieRuLinCombatTrackerPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar> CanonicalVars => [];

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is not { } player)
        {
            return;
        }

        // 每打一张牌（含作祟自己），本回合打牌计数 +1。
        await PowerCmd.Apply<CardsPlayedThisTurnPower>(
            choiceContext, player.Creature, 1m, player.Creature, null);
    }
}

