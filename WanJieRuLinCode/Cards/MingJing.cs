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
/// 明镜 —— 复制 1 张手牌，复制品费用 -1。升级：复制 2 张。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MingJing : WanJieRuLinCardModel
{
    public MingJing() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Copies", IsUpgraded ? 2m : 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is not { } p)
        {
            return;
        }

        var prefs = new CardSelectorPrefs(
            LocString.KeyPathToLocString("WAN_JIE_RU_LIN_CARD_MING_JING.choosePrompt"),
            IsUpgraded ? 2 : 1);
        var chosen = await CardSelectCmd.FromHand(choiceContext, p, prefs, null, this);

        foreach (var card in chosen)
        {
            var copy = card.CreateCloneForPlayer(p);
            copy.EnergyCost.SetCustomBaseCost(Math.Max(0, card.EnergyCost.Canonical - 1));
            await CardPileCmd.Add(copy, PileType.Hand, CardPilePosition.Random, null, false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Copies"].UpgradeValueBy(1m); // 1 → 2
    }
}
