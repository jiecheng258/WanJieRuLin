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
/// 薄雾 —— Power牌 1费
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class BaoWu : WanJieRuLinCardModel
{
    public BaoWu() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("EnergyPerTurn", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<MistPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.EnergyPerTurn = DynamicVars.GetIntOrDefault("EnergyPerTurn", 1);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["EnergyPerTurn"].UpgradeValueBy(1);    }
}
