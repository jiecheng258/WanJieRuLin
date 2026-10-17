using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using WanJieRuLin.Cards;

namespace WanJieRuLin.Powers;

// ============================================================================
// 回合 / 费用 / 状态操纵能力（v0.9）
//
//   - 灵魂 LingHunPower：每回合第 1 张技能牌 -1 费（升级前 2 张）
//   - 虚实界 XuShiJiePower：攻击牌无视 3 点格挡（升级 5）
//   - 墨心 MoXinPower：0 费牌伤害/格挡 +1（升级 +2）
//   - 虚空界 XuKongJiePower：每回合第 1 张牌打出后返回手牌（升级前 2 张）
//   - 墨生 MoShengPower：本回合每打 1 张牌 +1 格挡（升级 +2）
// ============================================================================

/// <summary>灵魂：每回合第 1 张技能牌 -1 费（升级前 2 张）。</summary>
[RegisterPower]
public sealed class LingHunPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("SkillsPerTurn", 1m),
        new DynamicVar("CostReduction", 1m),
    ];

    private int _skillsReducedThisTurn;

    private int SkillsPerTurn => IsUpgraded ? 2 : 1;

    public override bool TryModifyEnergyCostInCombat(
        CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        if (Owner?.Player is null)
        {
            modifiedCost = originalCost;
            return false;
        }

        // 只减「技能」牌，且本回合还没用完额度。
        if (card.Type != CardType.Skill || _skillsReducedThisTurn >= SkillsPerTurn)
        {
            modifiedCost = originalCost;
            return false;
        }

        // 注意：这个钩子在出牌前调用，扣额度要在 AfterCardPlayed 里做，
        // 否则同一张牌可能被反复判定。这里只做「是否满足条件」的预览判定，
        // 真正扣额度在 AfterCardPlayed。
        modifiedCost = Math.Max(0m, originalCost - 1m);
        return true;
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is { } player && cardPlay.Card.Type == CardType.Skill)
        {
            _skillsReducedThisTurn++;
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner is { } owner && participants.Contains(owner))
        {
            _skillsReducedThisTurn = 0;
        }
    }
}

/// <summary>虚实界：你的攻击牌命中时额外造成 3 点无法格挡的伤害（升级 5）。</summary>
[RegisterPower]
public sealed class XuShiJiePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Pierce", 3m),
    ];

    private decimal Pierce => IsUpgraded ? 5m : 3m;

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is not { } player || cardPlay.Card.Type != CardType.Attack)
        {
            return;
        }

        // 攻击牌命中目标后，额外造成 Pierce 点「无法格挡」的真实伤害。
        if (cardPlay.Target is not { } target)
        {
            return;
        }

        await CreatureCmd.Damage(
            choiceContext, target, Pierce, ValueProp.Unblockable, player.Creature, cardPlay.Card, cardPlay);
    }
}

/// <summary>墨心：你打出的 0 费牌，伤害/格挡 +1（升级 +2）。</summary>
[RegisterPower]
public sealed class MoXinPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Bonus", 1m),
    ];

    private decimal Bonus => IsUpgraded ? 2m : 1m;

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        return cardSource is { } c && c.EnergyCost.Canonical == 0 && dealer == Owner
            ? Bonus
            : 0m;
    }

    public override decimal ModifyBlockAdditive(
        Creature? target,
        decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        return cardSource is { } c && c.EnergyCost.Canonical == 0 && target == Owner
            ? Bonus
            : 0m;
    }
}

/// <summary>虚空界：每回合第 1 张牌打出后返回手牌（每张限 1 次；升级前 2 张）。</summary>
[RegisterPower]
public sealed class XuKongJiePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("CardsPerTurn", 1m),
    ];

    private int CardsPerTurn => IsUpgraded ? 2 : 1;

    private int _returnedThisTurn;

    // 记录已返回过的牌（每张牌限 1 次）。
    private readonly HashSet<CardModel> _returnedCards = [];

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is not { } player || _returnedThisTurn >= CardsPerTurn)
        {
            return;
        }

        if (cardPlay.Card is ZuoSui)
        {
            return;
        }

        // 每张牌限返回 1 次。
        if (!_returnedCards.Add(cardPlay.Card))
        {
            return;
        }

        _returnedThisTurn++;

        await CardPileCmd.Add(
            cardPlay.Card, PileType.Hand, CardPilePosition.Random, null, false);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner is { } owner && participants.Contains(owner))
        {
            _returnedThisTurn = 0;
        }
    }
}

/// <summary>墨生：本回合每打 1 张牌 +1 格挡（升级 +2）。</summary>
[RegisterPower]
public sealed class MoShengPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("BlockPerCard", 1m),
    ];

    private decimal BlockPerCard => IsUpgraded ? 2m : 1m;

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is not { } player)
        {
            return;
        }

        if (cardPlay.Card is ZuoSui)
        {
            return;
        }

        await CreatureCmd.GainBlock(player.Creature, BlockPerCard, ValueProp.Move, null);
    }
}

/// <summary>鬼墨甲：每回合开始 +3 格挡（升级 +5 格挡 +1 鬼气）。</summary>
[RegisterPower]
public sealed class GuiMoJiaPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Block", 3m),
    ];

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner?.Player)
        {
            return;
        }

        var block = IsUpgraded ? 5m : 3m;
        await CreatureCmd.GainBlock(player.Creature, block, ValueProp.Move, null);

        if (IsUpgraded)
        {
            await GhostQi.Gain(player, 1);
        }
    }
}

/// <summary>凌厉：每回合第 1 张攻击牌伤害 +3（升级 +6）。</summary>
[RegisterPower]
public sealed class LingLiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Bonus", 3m),
    ];

    private int _attacksBoostedThisTurn;

    private decimal Bonus => IsUpgraded ? 6m : 3m;

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (cardSource?.Type == CardType.Attack
            && dealer == Owner
            && _attacksBoostedThisTurn < 1)
        {
            return Bonus;
        }

        return 0m;
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type == CardType.Attack)
        {
            _attacksBoostedThisTurn++;
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner is { } owner && participants.Contains(owner))
        {
            _attacksBoostedThisTurn = 0;
        }
    }
}

/// <summary>墨移：本回合下一张牌费用 -N（一次性，打出后自删）。</summary>
[RegisterPower]
public sealed class MoYiNextCardPower : WanJieTurnScopedPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("CostReduction", 1m),
    ];

    /// <summary>本回合尚未消耗额度（只减下一张牌）。</summary>
    private bool _used;

    public override bool TryModifyEnergyCostInCombat(
        CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        if (_used || Owner?.Player is null)
        {
            modifiedCost = originalCost;
            return false;
        }

        modifiedCost = Math.Max(0m, originalCost - Amount);
        return true;
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!_used)
        {
            _used = true;
        }

        await PowerCmd.Remove(this);
    }
}
