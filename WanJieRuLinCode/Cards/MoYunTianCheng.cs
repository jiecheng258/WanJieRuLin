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
/// 每回合结束时，若鬼气在 3–7 之间，抽 2 张牌并获得 2 点能量。
/// ★ M 流引擎，每回合限 1 次（R5）。升级后抽 3 张。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MoYunTianCheng : WanJieRuLinCardModel
{
    public MoYunTianCheng() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(2),
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
        DynamicVars.Cards.UpgradeValueBy(1);    }
}
