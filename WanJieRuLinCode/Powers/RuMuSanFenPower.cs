using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using WanJieRuLin.Aspects;

namespace WanJieRuLin.Powers;

/// <summary>
/// 入木三分 —— ★ 纯线流增幅能力（v0.8）。
/// 每打出一张「线」牌，额外 +1 临时力量 +1 临时敏捷。
/// </summary>
[RegisterPower]
public sealed class RuMuSanFenPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is not { } player)
        {
            return;
        }
        if (WanJieAspectQuery.Of(cardPlay.Card) != WanJieAspect.Line)
        {
            return;
        }
        await PowerCmd.Apply<WanJieTempStrengthPower>(
            choiceContext, player.Creature, 1, player.Creature, null);
        await PowerCmd.Apply<WanJieTempDexterityPower>(
            choiceContext, player.Creature, 1, player.Creature, null);
    }
}
