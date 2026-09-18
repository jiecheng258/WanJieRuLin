using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 暂避锋芒 —— 技能牌 2 能量：本回合无法打出攻击牌，获得的格挡翻倍，
/// 下回合获得 2 点能量 1 点鬼气。升级后能量消耗为 1。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ZanBiFengMang : WanJieRuLinCardModel
{
    private const int EnergyNextTurn = 2;
    private const int GhostQiNextTurn = 1;

    public ZanBiFengMang() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Energy(EnergyNextTurn),
        GhostQiGainVarOf(GhostQiNextTurn)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<BideDefensivelyPower>(choiceContext, 1m);
        await ApplySelf<EnergyNextTurnPower>(
            choiceContext, DynamicVars.Energy.BaseValue);
        await ApplySelf<GhostQiNextTurnPower>(choiceContext, GhostQiNextTurn);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
