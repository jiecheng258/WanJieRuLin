using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using WanJieRuLin.Cards;

namespace WanJieRuLin.Powers;

// ============================================================================
// 作祟联动收益能力（v0.9）—— 打作祟 / 被作祟强化时触发额外收益。
//
//   - 墨瘾 MoYinPower：每打作祟抽 2 张（升级抽 3）
//   - 墨灵 MoLingPower：每打作祟 +1 力量（每回合上限 3；升级上限 5）
//   - 魂归 HunGuiPower：每打被作祟强化的牌抽 1 张（升级抽 2）
//   - 鬼息 GuiXiPower：每获得鬼气 +2 格挡（升级 +3）
// ============================================================================

/// <summary>墨瘾：每打作祟抽 2 张（升级抽 3）。</summary>
[RegisterPower]
public sealed class MoYinPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Draw", 2m),
    ];

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is not { } player)
        {
            return;
        }

        // 只对「作祟」牌触发。
        if (cardPlay.Card is not ZuoSui)
        {
            return;
        }

        await CardPileCmd.Draw(choiceContext, IsUpgraded ? 3 : 2, player);
    }
}

/// <summary>墨灵：每打作祟 +1 力量（每回合上限 3；升级上限 5）。</summary>
[RegisterPower]
public sealed class MoLingPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Cap", 3m),
        new DynamicVar("Strength", 1m),
    ];

    private int _triggeredThisTurn;

    private int Cap => IsUpgraded ? 5 : 3;

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is not { } player)
        {
            return;
        }

        if (cardPlay.Card is not ZuoSui)
        {
            return;
        }

        if (_triggeredThisTurn >= Cap)
        {
            return;
        }

        _triggeredThisTurn++;
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, player.Creature, 1m, player.Creature, null);
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

/// <summary>魂归：每打被作祟强化的牌抽 1 张（升级抽 2）。</summary>
[RegisterPower]
public sealed class HunGuiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Draw", 1m),
    ];

    /// <summary>
    /// 由 ZuoSuiPower 在给一张牌注入强化效果后调用（抽牌）。
    /// </summary>
    public async Task OnEnhancedCardPlayed(PlayerChoiceContext choiceContext, Player player)
    {
        await CardPileCmd.Draw(choiceContext, IsUpgraded ? 2 : 1, player);
    }
}

/// <summary>鬼息：每获得鬼气 +2 格挡（升级 +3）。</summary>
[RegisterPower]
public sealed class GuiXiPower : ModPowerTemplate, ISecondaryResourceHookListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Block", 2m),
    ];

    public async Task AfterSecondaryResourceChanged(SecondaryResourceChangeContext context)
    {
        // 只关心自己的鬼气「获得」（Delta > 0）。
        if (context.Player != Owner?.Player || context.Definition?.Id != ModResources.GhostQiId)
        {
            return;
        }

        if (context.Delta <= 0)
        {
            return;
        }

        var block = IsUpgraded ? 3m : 2m;
        await CreatureCmd.GainBlock(
            context.Player.Creature, block, ValueProp.Move, null);
    }

    public decimal ModifySecondaryResourceGain(SecondaryResourceChangeContext context, decimal amount) => amount;
    public decimal ModifyMaxSecondaryResource(SecondaryResourceChangeContext context, decimal amount) => amount;
    public decimal ModifySecondaryResourceCost(SecondaryResourceCostContext context, decimal cost) => cost;
    public decimal ModifySecondaryResourceCostLate(SecondaryResourceCostContext context, decimal cost) => cost;
    public int ModifySecondaryResourceXValue(SecondaryResourceXContext context, int value) => value;
    public bool ShouldGainSecondaryResource(SecondaryResourceChangeContext context, decimal amount) => true;
    public bool ShouldSpendSecondaryResource(SecondaryResourceSpendContext context) => true;
    public SecondaryResourceInsufficientPayment ModifySecondaryResourceInsufficientPayment(
        SecondaryResourceInsufficientPaymentContext context, SecondaryResourceInsufficientPayment payment) => payment;
    public SecondaryResourceShortfallResolution ResolveSecondaryResourceShortfall(
        SecondaryResourceShortfallResolutionContext context, SecondaryResourceShortfallResolution current) => current;
    public bool ShouldResetSecondaryResource(SecondaryResourceChangeContext context) => true;
    public Task AfterSecondaryResourceSpent(SecondaryResourceSpendContext context) => Task.CompletedTask;
    public Task AfterSecondaryResourceShortfallPayment(SecondaryResourceInsufficientPaymentContext context) => Task.CompletedTask;
    public Task AfterSecondaryResourceReset(SecondaryResourceChangeContext context) => Task.CompletedTask;
}

/// <summary>回墨链：本回合每打出 0 费牌抽 1 张（升级 2 张）。</summary>
[RegisterPower]
public sealed class HuiMoLianPower : WanJieTurnScopedPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Draw", 1m),
    ];

    public bool IsUpgraded { get; set; }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is not { } p)
        {
            return;
        }

        if (cardPlay.Card.EnergyCost.Canonical <= 0)
        {
            await CardPileCmd.Draw(choiceContext, IsUpgraded ? 2 : 1, p);
        }
    }
}
