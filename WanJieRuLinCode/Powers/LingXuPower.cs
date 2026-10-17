using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 灵虚：本回合你所有 0 费牌打出后，将一张 0 费复制（附[消耗]）放入弃牌堆，
/// 可免费再打一次。升级后 1 费牌也生效。
/// </summary>
[RegisterPower]
public sealed class LingXuPower : WanJieTurnScopedPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("CostThreshold", 0m),
    ];

    private decimal CostThreshold => IsUpgraded ? 1m : 0m;

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is not { } player)
        {
            return;
        }

        var card = cardPlay.Card;
        if (card is null || card.EnergyCost.Canonical > CostThreshold)
        {
            return;
        }

        // 造一张 0 费复制（附[消耗]）入弃牌堆。
        if (card.CombatState is not { } combat)
        {
            return;
        }

        var copy = combat.CreateCard(card, player);
        copy.EnergyCost.SetCustomBaseCost(0);
        copy.AddKeyword(CardKeyword.Exhaust);
        await CardPileCmd.AddGeneratedCardToCombat(
            copy, PileType.Discard, player, CardPilePosition.Random);
    }
}
