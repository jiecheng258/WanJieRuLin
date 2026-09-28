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
/// 本回合获得 3 点格挡。下回合开始时获得 2 点能量与 1 点鬼气。
/// 升级后 3 点能量。★ L 流过渡牌：用「暂避」换下回合的运转资源。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ZanBiFengMang : WanJieRuLinCardModel
{
    public ZanBiFengMang() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("EnergyNextTurn", 2),
        ModCardVars.Int("GhostQiNextTurn", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<BideDefensivelyPower>(choiceContext, 1m);
        await ApplySelf<EnergyNextTurnPower>(choiceContext, DynamicVars.GetIntOrDefault("EnergyNextTurn", 2));
        await ApplySelf<GhostQiNextTurnPower>(choiceContext, DynamicVars.GetIntOrDefault("GhostQiNextTurn", 1));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["EnergyNextTurn"].UpgradeValueBy(1);    }
}
