using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 我不玩了 —— 技能牌 X 鬼气：之后回合无法获得鬼气，接下来 X 张牌可以免费打出。
/// 升级后接下来 X+2 张牌可以免费打出。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class WoBuWanLe : WanJieRuLinCardModel
{
    private const int UpgradeBonus = 2;

    public WoBuWanLe() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        SetGhostQiCostX();
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("BonusFree", 0)
    ];

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
        DynamicVars["BonusFree"].UpgradeValueBy(UpgradeBonus);
    }
}
