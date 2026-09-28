using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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
/// 弃掉任意张手牌（X）。抽 X+2 张牌。升级后抽 X+3 张。
/// ★ 反无限（R1/R7）：原为 0 费且升级后产能量 → 净资源为正。现改 1 费、不产能量。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class PingXingShiJie : WanJieRuLinCardModel
{
    public PingXingShiJie() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("BonusDraw", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is not { } player)
        {
            return;
        }
        var hand = CardPile.GetCards(player, [PileType.Hand]).ToList();
        if (hand.Count == 0)
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.GetIntOrDefault("BonusDraw", 2), player);
            return;
        }
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 0, hand.Count);
        var picked = (await CardSelectCmd.FromHandForDiscard(choiceContext, player, prefs, null, this)).ToList();
        if (picked.Count > 0)
        {
            await CardCmd.Discard(choiceContext, picked);
        }
        var draw = picked.Count + DynamicVars.GetIntOrDefault("BonusDraw", 2);
        await CardPileCmd.Draw(choiceContext, draw, player);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BonusDraw"].UpgradeValueBy(1);    }
}
