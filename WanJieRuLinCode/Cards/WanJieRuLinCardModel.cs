using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Content;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 万界如林所有卡牌的基类。
///
/// 作用：
/// - 统一把卡图指到 res://WanJieRuLin/images/cards/{类名}.png。
/// - 承载鬼气（次要资源）的读写快捷方法。
/// - 通过 <see cref="ICardPlayStateContributor"/> 支持「本回合已打作祟」这类条件。
/// - 提供伤害 / 格挡 / 抽牌 / 施加能力 / 能量 等通用效果助手。
/// </summary>
public abstract class WanJieRuLinCardModel : ModCardTemplate,
    ICardPlayStateContributor
{
    protected WanJieRuLinCardModel(
        int energyCost,
        CardType type,
        CardRarity rarity,
        TargetType targetType,
        bool shouldShowInCardLibrary = true)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");



    /// <summary>手牌是否不多于 n 张（用于「墨尽」这类条件牌）。</summary>
    protected bool HandCountAtMost(int n)
    {
        if (Owner is not { } p)
        {
            return true;
        }

        return CardPile.GetCards(p, [PileType.Hand]).Count() <= n;
    }

    // ------------------------------------------------------------------
    // v0.9 作祟联动探测
    //
    // 大量牌需要「本回合已打过作祟」「本回合已获得鬼气」这类条件。
    // 用两个回合限定的标记能力（ZuoSuiPlayedThisTurnPower /
    // GhostQiGainedThisTurnPower）记录，回合结束自动清除。
    // ------------------------------------------------------------------

    /// <summary>本回合是否已打过「作祟」。</summary>
    protected bool ZuoSuiPlayedThisTurn =>
        Owner is { } p
        && p.Creature.GetPowerAmount<ZuoSuiPlayedThisTurnPower>() > 0;

    /// <summary>本回合是否已获得过鬼气。</summary>
    protected bool GhostQiGainedThisTurn =>
        Owner is { } p
        && p.Creature.GetPowerAmount<GhostQiGainedThisTurnPower>() > 0;

    /// <summary>本回合已打出的牌数（含本张打出之前的计数，用于「已打 2 张以上」条件）。</summary>
    protected int CardsPlayedThisTurn =>
        Owner is { } p ? p.Creature.GetPowerAmount<CardsPlayedThisTurnPower>() : 0;

    /// <summary>
    /// ★ 笔锋减费。
    ///
    /// 框架在每次需要「这张牌的当前费用」时都会调用这里
    /// （卡面显示 / 可打出判定 / 实际支付），所以只要返回扣减后的值，
    /// 减费就会同时体现在视觉与结算上。
    /// </summary>
    // ------------------------------------------------------------------
    // 可打出性：子类只需实现 PlayCondition，返回 false 即灰掉这张牌。
    //
    // 关键发现：原版 CardModel.CanPlay(out reason, out preventer) 没有 virtual，
    // 无法重写。但原版有 protected virtual bool IsPlayable，RitsuLib 的
    // CardModelCapabilityPatches+IsPlayablePatch 会对它做 Postfix，并调用
    // CardModelCapabilityHost.ApplyCanPlay(card, value)，后者转发到所有
    // ICardPlayStateContributor.CanPlay(card)。
    // ⇒ 所以「只实现 ICardPlayStateContributor.CanPlay」就够了，
    //   完全不需要也不能重写原版 CanPlay。
    // ------------------------------------------------------------------

    /// <summary>
    /// 子类重写以声明额外的可打出条件。返回 <c>null</c> 表示不干预。
    /// 返回 <c>false</c> 表示当前不可打出（牌会灰掉）。
    /// </summary>
    protected virtual bool? PlayCondition => null;

    Nullable<bool> ICardPlayStateContributor.CanPlay(CardModel card) => PlayCondition;

    bool ICardPlayStateContributor.HasTurnEndInHandEffect(CardModel card) => false;

    // ------------------------------------------------------------------
    // 通用效果助手：让每张牌的实现保持短小、可读。
    // ------------------------------------------------------------------

    /// <summary>鬼气费用数值变量的名字（对应卡面占位符 {GhostQiCost:...}）。</summary>
    public const string GhostQiCostVarName = "GhostQiCost";

    /// <summary>鬼气获得量变量的默认名字（对应卡面占位符 {GhostQiGain}）。</summary>
    protected const string GhostQiGainVar = "GhostQiGain";

    /// <summary>给这张牌挂上「耗费鬼气 N」的固定费用。</summary>
    protected void SetGhostQiCost(int amount)
    {
        this.SecondaryCosts().Set(ModResources.GhostQiId, amount);
    }

    /// <summary>给这张牌挂上「耗费全部鬼气」的 X 费用。</summary>
    protected void SetGhostQiCostX(int multiplier = 1)
    {
        this.SecondaryCosts().Set(ModResources.GhostQiId, SecondaryResourceCost.X(multiplier));
    }

    /// <summary>读取本张牌的 X 鬼气效果数值。</summary>
    protected int GhostQiXValue(CardPlay cardPlay)
    {
        var ledger = cardPlay.SecondaryResources();

        var value = ledger.Value(ModResources.GhostQiId);
        if (value > 0)
        {
            return value;
        }

        return ledger.Spent(ModResources.GhostQiId);
    }

    /// <summary>本张牌的鬼气费用是否为 X 型。</summary>
    protected bool GhostQiCostIsX(CardPlay cardPlay) =>
        cardPlay.SecondaryResources().CostsX(ModResources.GhostQiId);

    /// <summary>本张牌实际消耗的鬼气量。</summary>
    protected int GhostQiSpent(CardPlay cardPlay) =>
        cardPlay.SecondaryResources().Spent(ModResources.GhostQiId);

    /// <summary>构造一个「获得 N 点鬼气」的数值变量（用于卡面显示与提示）。</summary>
    protected static DynamicVar GhostQiGainVarOf(int amount) =>
        ModCardVars.Int(GhostQiGainVar, amount).WithSharedTooltip("WAN_JIE_RU_LIN_GHOST_QI");

    /// <summary>构造一个绑定鬼气资源的「费用」数值变量，用于在卡面文本里显示费用。</summary>
    protected static DynamicVar GhostQiCostVarOf(int amount) =>
        SecondaryResourceVars.ForLocal(
            GhostQiCostVarName, Entry.ModId, ModResources.GhostQiLocalId, amount);

    /// <summary>当前鬼气。</summary>
    protected int MyGhostQi => Owner is { } p ? GhostQi.Get(p) : 0;

    /// <summary>给自己加鬼气，并打上「本回合已获得鬼气」标记（供条件牌探测）。</summary>
    protected async Task GainGhostQi(int amount)
    {
        if (Owner is not { } p || amount <= 0)
        {
            return;
        }

        await GhostQi.Gain(p, amount);

        if (p.Creature is { } c)
        {
            await PowerCmd.Apply<GhostQiGainedThisTurnPower>(
                new HookPlayerChoiceContext(this, p.NetId, c.CombatState, GameActionType.Combat),
                c, 1m, c, this);
        }
    }

    /// <summary>给自己扣鬼气（不视为"支付费用"，用于卡牌副作用）。</summary>
    protected Task LoseGhostQi(int amount) =>
        Owner is { } p ? GhostQi.Lose(p, amount) : Task.CompletedTask;

    // ---- 伤害助手 ----

    /// <summary>对单个目标造成伤害（可多段）。</summary>
    protected Task DealDamage(PlayerChoiceContext ctx, Creature target, decimal amount, int hitCount = 1)
    {
        var cmd = DamageCmd.Attack(amount).FromCard(this, null);
        return cmd.Targeting(target).WithHitCount(hitCount).Execute(ctx);
    }

    /// <summary>对全体敌人造成伤害（可多段）。</summary>
    protected Task DealDamageToAll(PlayerChoiceContext ctx, decimal amount, int hitCount = 1)
    {
        if (CombatState is null)
        {
            return Task.CompletedTask;
        }

        return DamageCmd.Attack(amount)
            .FromCard(this, null)
            .TargetingAllOpponents(CombatState)
            .WithHitCount(hitCount)
            .Execute(ctx);
    }

    /// <summary>给自己加能量。</summary>
    protected Task GainEnergy(decimal amount) =>
        Owner is { } p ? PlayerCmd.GainEnergy(amount, p) : Task.CompletedTask;

    /// <summary>抽 count 张牌。</summary>
    protected Task<IEnumerable<CardModel>> Draw(PlayerChoiceContext ctx, int count) =>
        Owner is { } p ? CardPileCmd.Draw(ctx, count, p) : Task.FromResult<IEnumerable<CardModel>>([]);

    /// <summary>给自己上格挡。</summary>
    protected Task GainBlock(PlayerChoiceContext ctx, decimal amount) =>
        Owner is { } p
            ? CreatureCmd.GainBlock(p.Creature, amount, ValueProp.Move, null)
            : Task.CompletedTask;

    /// <summary>对自己施加一个能力。</summary>
    protected Task ApplySelf<TPower>(PlayerChoiceContext ctx, decimal amount)
        where TPower : PowerModel =>
        Owner is { } p
            ? PowerCmd.Apply<TPower>(ctx, p.Creature, amount, p.Creature, this)
            : Task.CompletedTask;

    /// <summary>对自己施加一个能力，并返回施加后的实例（用于写 DynamicVars）。</summary>
    protected async Task<TPower?> ApplySelfAndGet<TPower>(PlayerChoiceContext ctx, decimal amount)
        where TPower : PowerModel
    {
        if (Owner is not { } p)
        {
            return null;
        }

        return await PowerCmd.Apply<TPower>(ctx, p.Creature, amount, p.Creature, this);
    }

    /// <summary>对目标施加一个能力。</summary>
    protected Task ApplyTo<TPower>(PlayerChoiceContext ctx, Creature target, decimal amount)
        where TPower : PowerModel =>
        PowerCmd.Apply<TPower>(ctx, target, amount, Owner?.Creature, this);

    /// <summary>对场上的每个敌人施加一个能力。</summary>
    protected async Task ApplyToAll<TPower>(PlayerChoiceContext ctx, decimal amount)
        where TPower : PowerModel
    {
        foreach (var enemy in CombatState!.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<TPower>(ctx, enemy, amount, Owner.Creature, this);
        }
    }

    /// <summary>把一张新牌洗入抽牌堆（随机位置）。</summary>
    protected async Task ShuffleToDrawPile(CardModel card)
    {
        if (Owner is { } p)
        {
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Random, null, false);
        }
    }

    // ------------------------------------------------------------------
    // X 能量费用。
    //
    // ★ 原版约定（见原版 Whirlwind / Skewer / MultiCast / Tempest / Malaise）：
    //   1) 构造函数里能量费用传 0 —— 不是 -1！
    //   2) 重写 HasEnergyCostX 返回 true（它在 CardModel 上是 protected virtual）。
    //
    //   CardEnergyCost 是**懒构造**的：
    //       EnergyCost = new CardEnergyCost(this, CanonicalEnergyCost, HasEnergyCostX)
    //   所以只传 -1 而不重写 HasEnergyCostX，费用会被当成普通 0 费，
    //   之后任何 ResolveEnergyXValue() 都会抛
    //       InvalidOperationException: "This card does not have an X-cost."
    //   而这个异常发生在出牌流程内部，会直接打断 PlayCardAction
    //   （实机表现为「打出这张牌就报错/卡住」）。
    // ------------------------------------------------------------------

    /// <summary>
    /// 本张牌的能量费用是否为 X。
    ///
    /// 千万不要写成 <c>ResolveEnergyXValue() > 0</c>：非 X 费牌调用它会**抛异常**，
    /// 而不是返回 0。判定规格应读 <see cref="MegaCrit.Sts2.Core.Models.CardModel.HasEnergyCostX"/>。
    /// </summary>
    protected bool IsEnergyXCards => HasEnergyCostX;

    /// <summary>
    /// 本张牌的 X 能量实际值。非 X 费牌返回 0（不会抛异常）。
    /// </summary>
    protected int EnergyXValue => HasEnergyCostX ? ResolveEnergyXValue() : 0;
}
