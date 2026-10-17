using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 回天 —— 消耗。将本场战斗已消耗的 1 张牌返回手牌。升级：返回 2 张。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class HuiTian : WanJieRuLinCardModel
{
    public HuiTian() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Return", 1m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is not { } p)
        {
            return;
        }

        var exhausted = CardPile.GetCards(p, [PileType.Exhaust]).ToList();
        var count = Math.Min(IsUpgraded ? 2 : 1, exhausted.Count);

        var prefs = new CardSelectorPrefs(
            LocString.KeyPathToLocString("WAN_JIE_RU_LIN_CARD_HUI_TIAN.choosePrompt"), count);
        var chosen = await CardSelectCmd.FromCombatPile(
            choiceContext, p.PlayerCombatState.ExhaustPile, p, prefs);

        foreach (var card in chosen)
        {
            await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Random, this, false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Return"].UpgradeValueBy(1m); // 1 → 2
    }
}
