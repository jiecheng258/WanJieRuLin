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
/// 脱影 —— 抽 3 张牌，丢弃 1 张手牌。升级：抽 4 弃 1。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class TuoYing : WanJieRuLinCardModel
{
    public TuoYing() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Draw", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Draw(choiceContext, IsUpgraded ? 4 : 3);

        if (Owner is { } p)
        {
            var prefs = new CardSelectorPrefs(
                LocString.KeyPathToLocString("WAN_JIE_RU_LIN_CARD_TUO_YING.discardPrompt"), 1);
            var toDiscard = await CardSelectCmd.FromHandForDiscard(
                choiceContext, p, prefs, null, this);
            foreach (var card in toDiscard)
            {
                await CardCmd.Discard(choiceContext, card);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Draw"].UpgradeValueBy(1m); // 3 → 4
    }
}
