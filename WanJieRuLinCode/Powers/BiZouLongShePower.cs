using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using WanJieRuLin.Aspects;

namespace WanJieRuLin.Powers;

/// <summary>
/// 笔走龙蛇 —— ★ **纯点流**的能量引擎（v0.8.2 规范化）。
///
/// 效果：你每打出一张「点」牌，获得 {EnergyGain} 点能量。
///
/// 设计意图（点流派的核心经济模型）：
///   点牌本身数值偏低（0–1 费、伤害低于同费线牌），
///   用「每张点牌回 1 能量」补偿 —— 点牌 = 燃料，
///   让「只堆点牌」成为一条能持续出牌的滚雪球路线。
///
/// 触发时机：AfterCardPlayed（牌结算完成后）。
///   打出一张点牌 → 结算其伤害/抽牌 → 再回 1 能量。
///   因为是「打出后」结算，所以本牌自己消耗的能量不会立即回补，
///   下几张点牌才能享受到返还，形成「越点越顺」的节奏。
///
/// 收益规则：
///   - 每张点牌固定回 {EnergyGain} 点（不随点牌费用变化）
///   - 无每回合上限（点牌本身数值低，靠量取胜）
///   - 与「乱点」的负面并行：点得越多能量越多，但第 6 张起扣最大生命
///     → 这是点流的「甜与痛」——能量引擎的代价由乱点机制买单。
/// </summary>
[RegisterPower]
public sealed class BiZouLongShePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>每打出一张「点」牌获得的能量。</summary>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("EnergyGain", 1)
    ];

    /// <summary>每张点牌回的能量数（可配置，便于后续平衡或升级）。</summary>
    public int EnergyGain { get; set; } = 1;

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Owner 是 Creature，要经 .Player 拿到 Player（之前踩过 CS1061 的坑）。
        if (Owner?.Player is not { } player)
        {
            return;
        }

        // 只对「点」牌生效（能力牌本身、线牌、面牌都不触发）。
        if (WanJieAspectQuery.Of(cardPlay.Card) != WanJieAspect.Point)
        {
            return;
        }

        await PlayerCmd.GainEnergy(Math.Max(1, EnergyGain), player);
    }
}
