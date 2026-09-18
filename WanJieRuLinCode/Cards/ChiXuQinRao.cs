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
/// 持续侵扰 —— 攻击牌 2 能量、1 点鬼气：给予 1 层易伤、2 层虚弱，造成 14 点伤害。升级后造成 18 点伤害。
/// 按本模组汇率 1 鬼气 ≈ 1 能量，总成本 ≈ 3 能量；对比原版 3 费攻击牌区间（30–32 伤，无附加），
/// 本卡减掉易伤/虚弱附加后取 14。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ChiXuQinRao : WanJieRuLinCardModel
{
    public ChiXuQinRao() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        SetGhostQiCost(1);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 鬼气费用（卡面用 {GhostQiCost:secondaryResourceIcons()} 渲染成图标+数字）。
        GhostQiCostVarOf(1),
        new DamageVar(14m, ValueProp.Move),
        ModCardVars.Int("Vulnerable", 1),
        ModCardVars.Int("Weak", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await ApplyTo<VulnerablePower>(choiceContext, cardPlay.Target, DynamicVars.GetIntOrDefault("Vulnerable", 1));
        await ApplyTo<WeakPower>(choiceContext, cardPlay.Target, DynamicVars.GetIntOrDefault("Weak", 2));
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
