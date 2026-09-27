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
/// 每回合开始时，若鬼气不高于 2，获得 1 点能量。升级后 ★ 质变：2 点能量。
/// ★ L 流引擎。有每回合上限，且绑死低鬼气。
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
