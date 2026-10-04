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
/// 力透纸背 —— ★ **偏激流 · 纯线** 的支撑能力。
///
/// 效果：**每回合开始时获得 {Force} 点力道**。
///
/// 设计意图：让「只堆线牌」有一条稳定引擎 —— 力道本来是回合内玩法，
/// 这个能力把它变成每回合自动到账的底盘，于是线牌越打越厚。
///
/// 注：设计稿原本写「力道不再清空」，但那样会让数值无限累积（反无限红线）。
/// 改成「每回合固定到账」既保留了纯线的手感，又天然有上限。
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

    /// <summary>每回合自动获得的力道层数。</summary>
    public int Force { get; set; } = 2;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner is null || player.Creature != Owner)
        {
            return;
        }

        var amount = Math.Max(1, Force);
        await PowerCmd.Apply<LiDaoPower>(
            choiceContext, Owner, amount, Owner, null);
    }
}
