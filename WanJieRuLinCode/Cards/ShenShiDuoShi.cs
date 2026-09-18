using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 审时度势 —— 技能牌 X 点鬼气：获得 X+1 点能量，抽 1 张牌。升级后抽 2 张牌。
/// 把鬼气换成能量，是鬼气经济「变现」的手段。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ShenShiDuoShi : WanJieRuLinCardModel
{
    public ShenShiDuoShi() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        SetGhostQiCostX();
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var x = GhostQiXValue(cardPlay);
        await GainEnergy(x + 1);
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
