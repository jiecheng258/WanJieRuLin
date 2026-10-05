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
/// 张弛有度 —— 三类联系的桥：点牌产出的是手牌流量。
///
/// 效果：你打出「点」牌时，抽 1 张牌（每回合上限 {Cap} 次）。
/// </summary>
[RegisterPower]
public sealed class ZhangChiYouDuPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [ModCardVars.Int("Cap", 2)];

    public int Cap { get; set; } = 2;

    private int _usedThisTurn;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner is not null && player.Creature == Owner)
        {
            _usedThisTurn = 0;
        }
        await Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is not { } player || _usedThisTurn >= Math.Max(1, Cap))
        {
            return;
        }

        if (WanJieAspectQuery.Of(cardPlay.Card) != WanJieAspect.Point)
        {
            return;
        }

        _usedThisTurn++;
        await CardPileCmd.Draw(choiceContext, 1, player);
    }
}
