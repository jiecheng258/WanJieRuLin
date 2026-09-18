using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 幽然鬼火 —— 攻击牌 3 鬼气：造成 18 点伤害，给予 4 层易伤。
/// 结算顺序为先上易伤再结算伤害，因此 18 点伤害能吃到本次易伤加成。
/// 升级后伤害 18 → 24。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class YouRanGuiHuo : WanJieRuLinCardModel
{
    private const int GhostQiCost = 3;
    private const int VulnerableAmount = 4;

    public YouRanGuiHuo() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        SetGhostQiCost(GhostQiCost);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 鬼气费用（卡面用 {GhostQiCost:secondaryResourceIcons()} 渲染成图标+数字）。
        GhostQiCostVarOf(3),
        new DamageVar(18m, ValueProp.Move),
        ModCardVars.Power<VulnerablePower>(VulnerableAmount)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var target = cardPlay.Target;

        // 先上易伤再结算伤害：与升级版一致，且数值上更优。
        await ApplyTo<VulnerablePower>(choiceContext, target, VulnerableAmount);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // 原本是 UpgradeValueBy(0m)（空升级），改成实实在在的 +6 伤害。
        DynamicVars.Damage.UpgradeValueBy(6m);
    }
}
