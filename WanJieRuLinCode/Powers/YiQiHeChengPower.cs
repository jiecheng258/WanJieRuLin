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
/// 一气呵成 —— 三类联系的桥：面牌也给线/点垫数值。
///
/// 效果：你打出「面」牌时，本回合获得 {Amount} 点[gold]临时力量[/gold]。
/// </summary>
[RegisterPower]
public sealed class YiQiHeChengPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [ModCardVars.Int("Amount", 1)];

    public int Amount { get; set; } = 1;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is not { } player)
        {
            return;
        }

        if (WanJieAspectQuery.Of(cardPlay.Card) != WanJieAspect.Face)
        {
            return;
        }

        await PowerCmd.Apply<WanJieTempStrengthPower>(
            choiceContext, player.Creature, Math.Max(1, Amount), player.Creature, null);
    }
}
