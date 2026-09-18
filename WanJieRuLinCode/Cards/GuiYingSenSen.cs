using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 鬼影森森 —— 能力牌 2 能量：每造成 20 点伤害，获得 1 点鬼气。升级后每 15 点。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class GuiYingSenSen : WanJieRuLinCardModel
{
    public GuiYingSenSen() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Threshold", GuiYingSenSenPower.DamageThreshold)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<GuiYingSenSenPower>(choiceContext, 1);
        if (power is not null)
        {
            power.Threshold = DynamicVars.GetIntOrDefault("Threshold", GuiYingSenSenPower.DamageThreshold);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Threshold"].UpgradeValueBy(
            GuiYingSenSenPower.ReducedDamageThreshold - GuiYingSenSenPower.DamageThreshold);
    }
}
