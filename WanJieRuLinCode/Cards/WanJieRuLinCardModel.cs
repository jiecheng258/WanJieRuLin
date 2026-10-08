using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Content;
using WanJieRuLin.Aspects;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 万界如林所有卡牌的基类。
///
/// 作用：
/// - 统一把卡图指到 res://WanJieRuLin/images/cards/{类名}.png。
/// - 承载「点 · 线 · 面」框架：子类重写 <see cref="Aspect"/> 声明归属。
/// - 实现 <see cref="ICardEnergyCostContributor"/>，把「笔锋」变成实际减费。
/// - 通过 <see cref="ICardPlayStateContributor"/> 支持「力道 ≥5 才可打出」这类条件。
/// </summary>
public abstract class WanJieRuLinCardModel : ModCardTemplate,
    ICardPlayStateContributor,
    IWanJieAspectCard
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

    // ========================================================================
    // v0.5 「点 · 线 · 面」框架
    //
    // 所有攻击牌与技能牌都必须在子类里重写 Aspect 声明归属（点/线/面）；
    // 能力牌与打击/防御等基础牌保持默认的 None。
    // ========================================================================

    /// <summary>
    /// 本牌的笔法归属。子类重写：<c>public override WanJieAspect Aspect =&gt; WanJieAspect.Point;</c>
    /// </summary>
    public virtual WanJieAspect Aspect => WanJieAspect.None;

    /// <summary>本牌的归属（供框架内部与审计使用）。</summary>
    public WanJieAspect AspectValue => Aspect;


    // ------------------------------------------------------------------
    // v0.5 辅助方法（供生成的卡牌调用）
    // ------------------------------------------------------------------

    // ========================================================================
    // v0.7 三机制入口（点 / 线 / 面）
    // ========================================================================

    /// <summary>本回合已打出几张「点」牌（= 乱点层数）。</summary>
    protected int MyLuanDian => Owner is { } p ? LuanDianPower.Of(p.Creature) : 0;

    /// <summary>本回合已打出几张「线」牌。</summary>
    protected int MyQianJun => Owner is { } p ? QianJunPower.Of(p.Creature) : 0;

    /// <summary>本回合是否已打出不少于 n 张「线」牌（用于「一线天」这类条件牌）。</summary>
    protected bool QianJunAtLeast(int n) => MyQianJun >= n;

    /// <summary>
    /// 「点」牌打出时的统一入口：累积乱点；超过 3 层自动吃罚（力量/敏捷/虚弱/易伤/诅咒）。
    /// 所有点牌都应在 OnPlay 里调用它。
    /// </summary>
    protected async Task PointHit(PlayerChoiceContext ctx)
    {
        if (Owner is { } p)
        {
            await LuanDianPower.OnPointCardPlayed(ctx, p);
        }
    }

    /// <summary>
    /// 「线」牌打出时的统一入口：+1 临时力量、+1 临时敏捷、额外抽 1 张。
    /// 所有线牌都应在 OnPlay 里调用它。
    /// </summary>
    protected async Task LineHit(PlayerChoiceContext ctx)
    {
        if (Owner is { } p)
        {
            await QianJunPower.OnLineCardPlayed(ctx, p);
        }
    }

    /// <summary>
    /// ★ 「去年今日此门中」—— 面牌回流：
    /// 把**本牌的 0 费版本**放进弃牌堆。**每张面牌每场战斗只生效一次**。
    /// 所有面牌都应在 OnPlay 末尾调用它。
    /// </summary>
    protected async Task ReturnFaceZeroCostCopy(PlayerChoiceContext ctx)
    {
        if (Owner is not { } p || !WanJieFaceReturn.TryClaim(p, GetType()))
        {
            return;
        }

        // ★ 造牌姿势（v0.7.5 重写）：
        //   ① 用「安全反射」拿 ModelDb.Card<T>() —— 直接 GetMethod(name, Type.EmptyTypes)
        //      在有重载时会抛 AmbiguousMatchException（而不是返回 null），
        //      一旦抛出就会让整个 OnPlay 中断 → **所有面牌看起来完全没效果**（实机 bug）。
        //      改为先枚举所有 Card 重载、挑出「无参泛型」那一个。
        //   ② RunState.CreateCard(canonical, player)：一步做 ToMutable + 设 Owner + 登记 RunState
        //      （漏登记会报 "must be added to a CombatState" → 软锁）。
        //   ③ 入堆用 AddGeneratedCardToCombat —— 工程内已验证的「战斗中造牌入堆」入口
        //      （BeiLeiMao 曾用它把手牌造进 Hand 并长期稳定运行）。
        if (p.RunState is not MegaCrit.Sts2.Core.Runs.RunState runState)
        {
            return;
        }

        if (CardLookup is null)
        {
            return;
        }

        CardModel? canonical;
        try
        {
            canonical = CardLookup.MakeGenericMethod(GetType()).Invoke(null, null) as CardModel;
        }
        catch (Exception)
        {
            // 造牌失败不应连累这张牌本身的其它效果。
            return;
        }

        if (canonical is null)
        {
            return;
        }

        CardModel created;
        try
        {
            created = runState.CreateCard(canonical, p);

            // 0 费版本：SetCustomBaseCost 直接改「基础费用」（永久、不受洗牌影响）。
            created.EnergyCost.SetCustomBaseCost(0);

            await CardPileCmd.AddGeneratedCardToCombat(
                created, PileType.Discard, p, CardPilePosition.Random);
        }
        catch (Exception)
        {
            return;
        }
    }

    /// <summary>
    /// ModelDb 上「无参泛型 Card&lt;T&gt;()」的 <see cref="MethodInfo"/>。
    ///
    /// ★ 必须这样拿：`GetMethod("Card", Type.EmptyTypes)` 在存在重载时抛
    /// <see cref="System.Reflection.AmbiguousMatchException"/>，会让整张面牌的
    /// 打出流程中断（实机表现为「面牌完全没效果」）。
    /// </summary>
    private static readonly System.Reflection.MethodInfo? CardLookup =
        typeof(ModelDb)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .FirstOrDefault(m => m.Name == "Card"
                                 && m.IsGenericMethodDefinition
                                 && m.GetParameters().Length == 0);






    /// <summary>手牌是否不多于 n 张（用于「墨尽」这类条件牌）。</summary>
    protected bool HandCountAtMost(int n)
    {
        if (Owner is not { } p)
        {
            return true;
        }

        return CardPile.GetCards(p, [PileType.Hand]).Count() <= n;
    }

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

    /// <summary>当前鬼气。</summary>

    /// <summary>墨匀：鬼气落在均衡区间（3–7）。</summary>

    /// <summary>鬼气不高于 n。</summary>

    /// <summary>鬼气不低于 n。</summary>

    /// <summary>鬼气恰好为 n。</summary>

    // 设为 public，方便能力（如 HalfInkPower）复用同一套阈值，避免两处写死漂移。

    /// <summary>给自己加鬼气。</summary>

    /// <summary>给自己扣鬼气（不视为"支付费用"，用于卡牌副作用）。</summary>

    /// <summary>失去全部鬼气，返回实际失去量。</summary>

    /// <summary>把鬼气直接设为某个值。</summary>

    /// <summary>已知要失去的量，直接扣（用于「失去所有鬼气」后按量结算）。</summary>

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
