using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 斑驳乱点 —— 「点」牌的核心机制。
///
/// 效果：每打出一张「点」牌，本回合累积 1 层乱点。
/// - **3 层之内：毫无损耗**（鼓励连点）
/// - **第 4 层起**：每多 1 层，按顺序追加一项本回合临时负面（循环取用）：
///     临时力量 −2 / 临时敏捷 −2 / 虚弱 2 / 易伤 2 / 塞 1 张「原版诅咒卡」进弃牌堆
/// - **第 6 张起**：每多打 1 张，额外 −1 **最大生命**（永久，不随回合恢复）
/// - **回合结束，临时负面全部清空**（下回合干净重来），但**最大生命不恢复**
///
/// ★ 设计意图：点牌费用极低（0–1），用「本回合越点越乱」定价 ——
///   3 张以内克制、4–5 张吃临时减益、6 张起伤及根本（扣最大生命）。
///   这是**递进的决策点**，不是纯数值。
///
/// 继承 <see cref="WanJieTurnScopedPower"/> ：回合结束自动消失（临时负面随之清空）。
/// </summary>
[RegisterPower]
public sealed class LuanDianPower : WanJieTurnScopedPower
{
    /// <summary>安全的层数上限 —— 这个数字以内不产生任何负面。</summary>
    public const int SafeThreshold = 3;

    public override PowerType Type => PowerType.Debuff;

    /// <summary>可叠加：层数就是「本回合已点了几张」。</summary>
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("SafeThreshold", SafeThreshold)
    ];

    /// <summary>本回合的乱点层数。</summary>
    public static int Of(Creature? creature)
        => creature is null ? 0 : Math.Max(0, creature.GetPowerAmount<LuanDianPower>());

    /// <summary>
    /// 打出一张「点」牌时调用：层数 +1；若超过安全线，按超出量追加负面。
    /// </summary>
    public static async Task OnPointCardPlayed(
        PlayerChoiceContext ctx, Player player)
    {
        var next = Of(player.Creature) + 1;

        await PowerCmd.Apply<LuanDianPower>(
            ctx, player.Creature, 1, player.Creature, null);

        // ★ v0.8.3：「点」牌自带被动能量回馈 —— 每打出一张点牌，无条件 +1 能量。
        //   这是点流的核心经济：点牌数值偏低（0–1 费），用回能量补偿，点牌 = 燃料。
        //   代价由本机制下方的「乱点」负面买单（第 4 张起临时减益、第 6 张起扣最大生命）。
        //   「笔走龙蛇」能力会把这里的基础回馈从 1 强化到 2（见 BiZouLongShePower）。
        await PlayerCmd.GainEnergy(1, player);

        var over = next - SafeThreshold;
        if (over <= 0)
        {
            return;
        }

        // 第 4 层起，每层追加一项负面（循环取用）。
        //
        // ★ v0.7：负面幅度**翻倍**，且第 5 项改为**塞一张原版诅咒卡进弃牌堆**
        //（用户：「你这个循环代价太低，还要增强负面效果」）
        // ★ v0.7.1：负面从「永久」改成「本回合临时」—— 用户反馈
        //   「不是永久降低力量敏捷，这样代价太大了」。
        //   现在用 WanJieTempStrengthPower / WanJieTempDexterityPower（回合结束消失）。
        for (var k = 0; k < over; k++)
        {
            switch (k % 5)
            {
                case 0:
                    // 临时力量 −2（回合结束消失）
                    await PowerCmd.Apply<WanJieTempStrengthPower>(
                        ctx, player.Creature, -2, player.Creature, null);
                    break;

                case 1:
                    // 临时敏捷 −2（回合结束消失）
                    await PowerCmd.Apply<WanJieTempDexterityPower>(
                        ctx, player.Creature, -2, player.Creature, null);
                    break;

                case 2:
                    // 虚弱 2
                    await PowerCmd.Apply<WeakPower>(
                        ctx, player.Creature, 2, player.Creature, null);
                    break;

                case 3:
                    // 易伤 2
                    await PowerCmd.Apply<VulnerablePower>(
                        ctx, player.Creature, 2, player.Creature, null);
                    break;

                default:
                    // ★ 塞一张**原版诅咒卡**进弃牌堆（四张轮换）
                    await AddCurseToDiscard(ctx, player, k / 5);
                    break;
            }
        }

        // ★ v0.7.5 加强（用户 #4）：「超过安全值之后，打出到第三张起开始扣除血量上限」。
        //   即：本回合第 6 张及以后**每多打 1 张**，额外 −1 最大生命（永久，不随回合恢复）。
        //
        //   ★ 注意：这里必须用「增量」，不能用「累计超出量」。
        //      over 是当前累计超出量（第 6 张=3、第 7 张=4…），
        //      若扣 over-2，会重复扣（第 7 张扣 2、第 8 张扣 3…），血量雪崩。
        //      「每多打 1 张扣 1」→ 固定扣 1。
        if (over >= 3)
        {
            await CreatureCmd.LoseMaxHp(ctx, player.Creature, 1, true);
        }
    }

    /// <summary>原版诅咒卡（轮换塞入弃牌堆）。</summary>
    private static readonly Type[] CurseCycle =
    [
        typeof(MegaCrit.Sts2.Core.Models.Cards.Doubt),
        typeof(MegaCrit.Sts2.Core.Models.Cards.Shame),
        typeof(MegaCrit.Sts2.Core.Models.Cards.Injury),
        typeof(MegaCrit.Sts2.Core.Models.Cards.Decay),
    ];

    /// <summary>
    /// 把一张原版诅咒卡放进弃牌堆。
    ///
    /// ★ 造牌姿势（v0.7.6 修正）：
    ///   用 CombatState.CreateCard(canonical, player) —— 战斗内造牌的正确入口，
    ///   一步做 ToMutable + 设 Owner + 登记 CombatState。
    ///   之前用 Activator.CreateInstance + ToMutable，漏登记 CombatState，
    ///   入堆时报 "must be added to a CombatState"（软锁）。
    /// </summary>
    private static async Task AddCurseToDiscard(
        PlayerChoiceContext ctx, Player player, int index)
    {
        if (player.Creature.CombatState is not MegaCrit.Sts2.Core.Combat.CombatState combat)
        {
            return;
        }

        var type = CurseCycle[Math.Abs(index) % CurseCycle.Length];
        if (Activator.CreateInstance(type) is not CardModel canonical)
        {
            return;
        }

        var created = combat.CreateCard(canonical, player);

        await CardPileCmd.AddGeneratedCardToCombat(
            created, PileType.Discard, player, CardPilePosition.Random);
    }
}
