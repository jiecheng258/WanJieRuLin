using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 千钧一线 —— 「线」牌的核心机制。
///
/// 效果：每打出一张「线」牌：
/// - 本回合获得 **1 点临时力量** 与 **1 点临时敏捷**（回合结束消失）
/// - **额外抽 1 张牌**（所有线牌都带，原本抽 4 的牌因此变抽 5）
///
/// ★ 设计意图：线是「放大器」——
///   临时力量让本回合后续的攻击更痛，临时敏捷让格挡更厚，
///   额外抽 1 保证手牌不断档。于是「线 → 点/面」的组合自然成立：
///   先用线把本回合的数值垫高，再用点（便宜）或面（高费高值）打出去。
///
/// 继承 <see cref="WanJieTurnScopedPower"/>：临时数值回合结束即消失。
/// </summary>
[RegisterPower]
public sealed class QianJunPower : WanJieTurnScopedPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("TempStat", 1)
    ];

    /// <summary>本回合已打出几张线牌（也就是临时数值的层数）。</summary>
    public static int Of(MegaCrit.Sts2.Core.Entities.Creatures.Creature? creature)
        => creature is null ? 0 : Math.Max(0, creature.GetPowerAmount<QianJunPower>());

    /// <summary>
    /// 打出一张「线」牌时调用：给临时力量/敏捷，并额外抽 1 张。
    /// </summary>
    public static async Task OnLineCardPlayed(
        PlayerChoiceContext ctx, Player player)
    {
        await PowerCmd.Apply<QianJunPower>(
            ctx, player.Creature, 1, player.Creature, null);

        // 临时力量 + 临时敏捷（本回合）
        await PowerCmd.Apply<WanJieTempStrengthPower>(
            ctx, player.Creature, 1, player.Creature, null);
        await PowerCmd.Apply<WanJieTempDexterityPower>(
            ctx, player.Creature, 1, player.Creature, null);

        // 所有线牌额外抽 1 张
        await CardPileCmd.Draw(ctx, 1, player);
    }
}
