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
/// 墨涌 —— 获得 1 鬼气，抽 2 张牌，丢弃 1 张手牌。升级：2 鬼气，抽 3 弃 1。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MoYun : WanJieRuLinCardModel
{
    public MoYun() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        GhostQiGainVarOf(1),
        new DynamicVar("Draw", IsUpgraded ? 3m : 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainGhostQi(IsUpgraded ? 2 : 1);
        await Draw(choiceContext, IsUpgraded ? 3 : 2);

        // 丢弃 1 张手牌（玩家选择）。
        if (Owner is { } p)
        {
            var prefs = new CardSelectorPrefs(
                LocString.KeyPathToLocString("WAN_JIE_RU_LIN_CARD_MO_YUN.discardPrompt"), 1);
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
        DynamicVars[GhostQiGainVar].UpgradeValueBy(1m); // 1 → 2
        DynamicVars["Draw"].UpgradeValueBy(1m);          // 2 → 3
    }
}
