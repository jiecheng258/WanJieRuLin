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
/// - **第 4 层起**：每多 1 层，按顺序追加一项负面惩罚：
///     力量 −1 / 敏捷 −1 / 虚弱 1 / 易伤 1 / 抽 1 张「乱墨」（诅咒牌）
/// - **回合结束全部清空**（负面是「本回合」的，下回合干净重来）
///
/// ★ 设计意图：点牌费用极低（0–1），所以用「本回合越点越乱」来定价 ——
///   玩家要么克制在 3 张以内，要么为了爆发主动吃惩罚。这是**决策点**，不是纯数值。
///
/// 继承 <see cref="WanJieTurnScopedPower"/> ：回合结束自动消失（负面随之清空）。
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

        var over = next - SafeThreshold;
        if (over <= 0)
        {
            return;
        }

        // 第 4 层起，每层追加一项负面（循环取用）。
        //
        // ★ v0.7：负面幅度**翻倍**，且第 5 项改为**塞一张原版诅咒卡进弃牌堆**
        //（用户：「你这个循环代价太低，还要增强负面效果」）。
        for (var k = 0; k < over; k++)
        {
            switch (k % 5)
            {
                case 0:
                    // 力量 −2
                    await PowerCmd.Apply<StrengthPower>(
                        ctx, player.Creature, -2, player.Creature, null);
                    break;

                case 1:
                    // 敏捷 −2
                    await PowerCmd.Apply<DexterityPower>(
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
    /// ★ 造牌姿势（沿用工程里已验证的写法）：
    ///   规范实例 → ToMutable() → 设 Owner → 交给 AddGeneratedCardToCombat。
    ///   三者缺一不可：不 ToMutable 会抛 CanonicalModelException；
    ///   手工克隆会踩「牌没有堆」的 NullReferenceException。
    /// </summary>
    private static async Task AddCurseToDiscard(
        PlayerChoiceContext ctx, Player player, int index)
    {
        var type = CurseCycle[Math.Abs(index) % CurseCycle.Length];
        if (Activator.CreateInstance(type) is not CardModel canonical)
        {
            return;
        }

        var created = canonical.ToMutable();
        created.Owner = player;

        await CardPileCmd.AddGeneratedCardToCombat(
            created, PileType.Discard, player, CardPilePosition.Random);
    }
}
