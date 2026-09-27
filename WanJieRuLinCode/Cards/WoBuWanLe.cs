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
/// 耗费所有鬼气。接下来 X 张牌可以免费打出。[消耗]
/// ★ 反无限：原本**无 [消耗]** → 无限元凶。升级后免费牌 +2。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class WoBuWanLe : WanJieRuLinCardModel
{
    public WoBuWanLe() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        SetGhostQiCostX();
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("BonusFree", 0)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var x = GhostQiXValue(cardPlay);
        var freeCards = x + DynamicVars.GetIntOrDefault("BonusFree", 0);
        if (freeCards <= 0)
        {
            return;
        }
        var power = await ApplySelfAndGet<WoBuWanLePower>(choiceContext, 1m);
        if (power is not null)
        {
            power.FreeCardsRemaining = freeCards;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BonusFree"].UpgradeValueBy(2);    }
}
