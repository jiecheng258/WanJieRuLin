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
/// 你接下来的回合不再获得能量，改为获得同等数值的鬼气。每回合开始时额外抽 1 张牌。
/// ★ 达佛给予的能力卡，到手即为升级状态。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class GuiQiSenSen : WanJieRuLinCardModel
{
    public GuiQiSenSen() : base(3, CardType.Power, CardRarity.Event, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<EnergyToGhostQiPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.Configure(EnergyToGhostQiPower.Permanent);
            power.DrawPerTurn = DynamicVars.Cards.IntValue;
        }
    }

    protected override void OnUpgrade()
    {
    }
}
