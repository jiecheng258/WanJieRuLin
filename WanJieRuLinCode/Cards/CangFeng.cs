using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 藏锋 —— 技能牌 0 能量：[耗费 1 点鬼气]，抽 2 张牌。
///
/// ★ 反无限（R1 / R7）—— 旧写法是「0 费 → **1 能量 + 2 张牌**」，纯白嫖：
///   0 费本身不花代价，还净赚能量与手牌，配上任何产鬼气的牌即可循环。
///   现在**去掉产能量**（R7：抽牌与产能量的组合即净资源为正），
///   只保留「用鬼气换手牌」这一件事。
///
///   它仍然可以是 0 费 —— 因为**鬼气费本身就是代价**，
///   而鬼气只能靠 ≥1 费（或 [消耗]）的牌产出（R6），所以燃料不免费，
///   无法自我维持。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class CangFeng : WanJieRuLinCardModel
{
    private const int GhostQiCost = 1;

    public CangFeng() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        SetGhostQiCost(GhostQiCost);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        GhostQiCostVarOf(GhostQiCost),
        ModCardVars.Cards(2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade()
    {
        // ★ 质变：抽 2 → 抽 3。
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
