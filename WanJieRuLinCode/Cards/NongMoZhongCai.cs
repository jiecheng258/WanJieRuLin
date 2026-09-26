using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 浓墨重彩 —— Power牌 3费
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class NongMoZhongCai : WanJieRuLinCardModel
{
    public NongMoZhongCai() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("MaxBlockPerTurn", 12)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<RichInkPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.MaxBlockPerTurn = DynamicVars.GetIntOrDefault("MaxBlockPerTurn", 12);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxBlockPerTurn"].UpgradeValueBy(4);    }
}
