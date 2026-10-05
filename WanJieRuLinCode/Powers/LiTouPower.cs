using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 力透纸背 —— ★ **纯线流**的支撑能力（v0.7 版）。
///
/// 效果：每回合开始时获得 {Force} 点[gold]临时力量[/gold]。
///
/// 设计意图：让线流有一条稳定底盘 —— 每回合白给临时力量，
/// 线牌（千钧一线本身还 +1 临时力/敏 + 抽 1）就越打越厚。
/// </summary>
[RegisterPower]
public sealed class LiTouPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Force", 2)
    ];

    /// <summary>每回合自动获得的临时力量层数。</summary>
    public int Force { get; set; } = 2;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner is null || player.Creature != Owner)
        {
            return;
        }

        var amount = Math.Max(1, Force);
        await PowerCmd.Apply<WanJieTempStrengthPower>(
            choiceContext, Owner, amount, Owner, null);
    }
}
