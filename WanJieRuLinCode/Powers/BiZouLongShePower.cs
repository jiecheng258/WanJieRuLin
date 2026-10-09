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
/// 笔走龙蛇 —— ★ **纯点流**的强化能力（v0.8.3 起，改为「强化」而非「本体」）。
///
/// 效果：你每打出一张「点」牌，额外获得 {EnergyGain} 点能量。
///
/// 与「点牌自带回馈」的关系：
///   - 点牌**本身**就自带「每张 +1 能量」（见 LuanDianPower.OnPointCardPlayed，无条件回馈）
///   - 本能力是**强化**：在自带 +1 的基础上，每张再额外 +{EnergyGain}
///   - 所以拥有本能力时，每张点牌实际回 (1 + EnergyGain) 点能量
///
/// 设计意图：点流不再依赖「先打出能力牌」才能启动 —— 点牌天生就是燃料，
/// 能力牌只是把燃料效率翻倍。这降低了点流的启动门槛，同时保留了「能力牌
/// 强化」的构筑深度。
///
/// 触发时机：AfterCardPlayed（牌结算完成后），只对「点」牌生效。
/// 收益规则：每张点牌额外 +{EnergyGain}（无每回合上限；代价仍由「乱点」买单）。
/// </summary>
[RegisterPower]
public sealed class BiZouLongShePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>每张点牌在基础 +1 之上额外回的能量。</summary>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("EnergyGain", 1)
    ];

    /// <summary>每张点牌额外回的能量数（可配置，便于平衡/升级）。</summary>
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
