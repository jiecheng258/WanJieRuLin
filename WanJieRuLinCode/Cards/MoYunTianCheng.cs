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
/// 墨韵天成 —— Power牌 2费
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MoYunTianCheng : WanJieRuLinCardModel
{
    public MoYunTianCheng() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("DrawPerTurn", 2),
        ModCardVars.Int("EnergyPerTurn", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<InkRhythmPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.DrawPerTurn = DynamicVars.GetIntOrDefault("DrawPerTurn", 2);
            power.EnergyPerTurn = DynamicVars.GetIntOrDefault("EnergyPerTurn", 2);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DrawPerTurn"].UpgradeValueBy(1);
        DynamicVars["EnergyPerTurn"].UpgradeValueBy(1);    }
}
