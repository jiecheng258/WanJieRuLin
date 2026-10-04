using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using WanJieRuLin.Aspects;

namespace WanJieRuLin.Powers;

/// <summary>
/// 纵横 —— 「线」牌的**滚雪球件**。
///
/// 效果：本回合内，你**每打出一张「线」牌，获得 {Amount} 点[gold]力道[/gold]**。
///
/// 设计意图：让线流产生正反馈 —— 越打线牌力道越厚，力道越厚线牌越强。
/// 本回合结束后自动消失（继承 <see cref="WanJieTurnScopedPower"/>）。
///
/// ★ 实现说明：原设计想挂「每当获得力道时额外 +N」，但引擎没有
/// AfterPowerApplied 这个可重写钩子（实测报「没有找到适合的方法来重写」）。
/// 改用已验证的 <see cref="AfterCardPlayed"/>，手感等价且更直观。
/// </summary>
[RegisterPower]
public sealed class ZongHengPower : WanJieTurnScopedPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Amount", 1)
    ];

    /// <summary>每打出一张线牌获得的力道层数。</summary>
    public int Amount { get; set; } = 1;

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is null)
        {
            return;
        }

        // 只对「线」牌生效。
        if (WanJieAspectQuery.Of(cardPlay.Card) != WanJieAspect.Line)
        {
            return;
        }

        var n = Math.Max(1, Amount);
        await PowerCmd.Apply<LiDaoPower>(choiceContext, Owner, n, Owner, null);
    }
}
