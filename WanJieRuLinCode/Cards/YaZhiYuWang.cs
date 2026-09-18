using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 压制欲望 —— 技能牌 1 鬼气：下回合获得 3 点鬼气。升级后下回合获得 4 点鬼气。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class YaZhiYuWang : WanJieRuLinCardModel
{
    private const int GhostQiCost = 1;
    private const int NextTurnGhostQi = 3;

    public YaZhiYuWang() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        SetGhostQiCost(GhostQiCost);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 鬼气费用（卡面用 {GhostQiCost:secondaryResourceIcons()} 渲染成图标+数字）。
        GhostQiCostVarOf(1),
        GhostQiGainVarOf(NextTurnGhostQi)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var amount = DynamicVars.GetIntOrDefault(GhostQiGainVar, NextTurnGhostQi);
        if (amount > 0)
        {
            await ApplySelf<GhostQiNextTurnPower>(choiceContext, amount);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[GhostQiGainVar].UpgradeValueBy(1);
    }
}
