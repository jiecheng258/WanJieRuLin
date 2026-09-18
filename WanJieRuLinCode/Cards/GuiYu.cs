using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 鬼域 —— 能力牌 9 鬼气：每回合第 1 张攻击牌伤害翻倍，
/// 第 1 张技能牌免费打出并抽 2 张牌，第 3 张牌本回合费用变为 0，第 4 张牌获得重放。
/// 升级后鬼气费用 9 → 7。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class GuiYu : WanJieRuLinCardModel
{
    private const int GhostQiCost = 9;
    private const int UpgradedGhostQiCost = 7;

    public GuiYu() : base(0, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        SetGhostQiCost(GhostQiCost);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 让「耗费 N 点鬼气」这行随升级自动从 9 变成 7。
        GhostQiCostVarOf(GhostQiCost)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<GuiYuPower>(choiceContext, 1m);
    }

    protected override void OnUpgrade()
    {
        // 鬼气费用 9 → 7。
        SetGhostQiCost(UpgradedGhostQiCost);
        DynamicVars[GhostQiCostVarName].UpgradeValueBy(UpgradedGhostQiCost - GhostQiCost);
    }
}
