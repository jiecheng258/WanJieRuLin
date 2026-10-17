using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using WanJieRuLin.Cards;

namespace WanJieRuLin.Powers;

// ============================================================================
// 鬼气经济能力（v0.9）—— 产鬼气 / 回鬼气，保证作祟永远有燃料。
//
//   - 鬼墨心 GuiMoXinPower：每回合开始 +1 鬼气（升级 +2）
//   - 鬼魄   GuiPoPower：每场战斗开始 +3 鬼气（升级 +4）
//   - 心墨   XinMoPower：每打 0 费牌 +1 鬼气（每回合上限 2；升级上限 3 且抽 1）
//   - 鬼影   GuiYingPower：每打 2 费牌 +1 鬼气（每回合上限 2；升级上限 4）
//   - 残影   CanYingPower：回合结束若没打作祟 +2 鬼气（升级 +3）
//   - 墨神   MoShenPower：每回合开始 +1 能量（升级 +1 能量 +1 鬼气）
// ============================================================================

/// <summary>鬼墨心：每回合开始 +1 鬼气（升级 +2）。</summary>
[RegisterPower]
public sealed class GuiMoXinPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>由卡牌 OnPlay 时设置，区分升级前后效果。</summary>
    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("GhostQiPerTurn", 1m),
    ];

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner?.Player)
        {
            return;
        }

        await GhostQi.Gain(player, IsUpgraded ? 2 : 1);
    }
}

/// <summary>鬼魄：每场战斗开始 +3 鬼气（升级 +4）。</summary>
[RegisterPower]
public sealed class GuiPoPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("GhostQi", 3m),
    ];

    public override async Task BeforeCombatStart()
    {
        if (Owner?.Player is not { } player)
        {
            return;
        }

        await GhostQi.Gain(player, IsUpgraded ? 4 : 3);
    }
}

/// <summary>心墨：每打 0 费牌 +1 鬼气（每回合上限 2；升级上限 3 且抽 1）。</summary>
[RegisterPower]
public sealed class XinMoPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Cap", 2m),
        new DynamicVar("GhostQiPerProc", 1m),
    ];

    /// <summary>本回合已触发次数。</summary>
    private int _triggeredThisTurn;

    private int Cap => IsUpgraded ? 3 : 2;

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is not { } player || cardPlay.Card is ZuoSui)
        {
            return;
        }

        if (cardPlay.Card.EnergyCost.Canonical != 0)
        {
            return;
        }

        if (_triggeredThisTurn >= Cap)
        {
            return;
        }

        _triggeredThisTurn++;
        await GhostQi.Gain(player, 1);

        if (IsUpgraded)
        {
            await CardPileCmd.Draw(choiceContext, 1, player);
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner is { } owner && participants.Contains(owner))
        {
            _triggeredThisTurn = 0;
        }
    }
}

/// <summary>鬼影：每打 2 费牌 +1 鬼气（每回合上限 2；升级上限 4）。</summary>
[RegisterPower]
public sealed class GuiYingPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Cap", 2m),
        new DynamicVar("GhostQiPerProc", 1m),
    ];

    private int _triggeredThisTurn;

    private int Cap => IsUpgraded ? 4 : 2;

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is not { } player || cardPlay.Card is ZuoSui)
        {
            return;
        }

        if (cardPlay.Card.EnergyCost.Canonical != 2)
        {
            return;
        }

        if (_triggeredThisTurn >= Cap)
        {
            return;
        }

        _triggeredThisTurn++;
        await GhostQi.Gain(player, 1);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner is { } owner && participants.Contains(owner))
        {
            _triggeredThisTurn = 0;
        }
    }
}

/// <summary>残影：回合结束若本回合没打作祟 +2 鬼气（升级 +3）。</summary>
[RegisterPower]
public sealed class CanYingPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("GhostQi", 2m),
    ];

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner?.Player is not { } player || !participants.Contains(Owner))
        {
            return;
        }

        // 本回合没打过作祟才给鬼气。
        if (player.Creature.GetPowerAmount<ZuoSuiPlayedThisTurnPower>() > 0)
        {
            return;
        }

        await GhostQi.Gain(player, IsUpgraded ? 3 : 2);
    }
}

/// <summary>墨神：每回合开始 +1 能量（升级 +1 能量 +1 鬼气）。</summary>
[RegisterPower]
public sealed class MoShenPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("EnergyPerTurn", 1m),
    ];

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner?.Player)
        {
            return;
        }

        await PlayerCmd.GainEnergy(1m, player);

        if (IsUpgraded)
        {
            await GhostQi.Gain(player, 1);
        }
    }
}

/// <summary>万界来朝：每场战斗开始 +2 鬼气 + 抽 2（升级 +3 鬼气 + 抽 3）。</summary>
[RegisterPower]
public sealed class WanJieLaiChaoPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("GhostQi", 2m),
        new DynamicVar("Draw", 2m),
    ];

    public override async Task BeforeCombatStart()
    {
        if (Owner?.Player is not { } player)
        {
            return;
        }

        await GhostQi.Gain(player, IsUpgraded ? 3 : 2);
        await CardPileCmd.Draw(
            new HookPlayerChoiceContext(this, player.NetId, CombatState, GameActionType.Combat),
            IsUpgraded ? 3 : 2, player);
    }
}
