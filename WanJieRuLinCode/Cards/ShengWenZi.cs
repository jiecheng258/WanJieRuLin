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
/// 圣文字 —— 攻击牌 1 能量 1 鬼气：造成 6 点伤害，给予 2 层易伤，给予 2 层虚弱。
/// 升级后造成 10 点伤害。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ShengWenZi : WanJieRuLinCardModel
{
    private const int GhostQiCost = 1;
    private const int VulnerableAmount = 2;
    private const int WeakAmount = 2;

    public ShengWenZi() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        SetGhostQiCost(GhostQiCost);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 鬼气费用（卡面用 {GhostQiCost:secondaryResourceIcons()} 渲染成图标+数字）。
        GhostQiCostVarOf(1),
        new DamageVar(6m, ValueProp.Move),
        ModCardVars.Power<VulnerablePower>(VulnerableAmount),
        ModCardVars.Power<WeakPower>(WeakAmount)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var target = cardPlay.Target;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .Execute(choiceContext);

        await ApplyTo<VulnerablePower>(choiceContext, target, VulnerableAmount);
        await ApplyTo<WeakPower>(choiceContext, target, WeakAmount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
