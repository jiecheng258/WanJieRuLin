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
using WanJieRuLin.Aspects;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// ★ **点** —— 弃 1 张牌，获得 3 点[gold]笔锋[/gold]。升级后 4 点。
/// ★ 把打不出的牌换成费用优势。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ShuSan : WanJieRuLinCardModel
{
    public ShuSan() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Point;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Discard", 1),
        ModCardVars.Int("Edge", 3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is not { } p)
        {
            return;
        }
        var hand = CardPile.GetCards(p, [PileType.Hand]).ToList();
        if (hand.Count == 0)
        {
            return;
        }
        var n = Math.Min(DynamicVars.GetIntOrDefault("Discard", 1), hand.Count);
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, n);
        var picked = (await CardSelectCmd.FromHandForDiscard(choiceContext, p, prefs, null, this)).ToList();
        if (picked.Count > 0)
        {
            await CardCmd.Discard(choiceContext, picked);
        }
        await GainBiFeng(choiceContext, DynamicVars.GetIntOrDefault("Edge", 3));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Edge"].UpgradeValueBy(1);    }
}
