using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using STS2RitsuLib.Cards.DynamicVars;

namespace WanJieRuLin.Cards;

/// <summary>
/// 侵蚀 —— 攻击牌 0 能量、1 点鬼气：给予 1 层易伤，造成 8 点伤害。升级后给予 2 层易伤。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class QinShi : WanJieRuLinCardModel
{
    private const int VulnerableAmount = 1;

    public QinShi() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        SetGhostQiCost(1);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 鬼气费用（卡面用 {GhostQiCost:secondaryResourceIcons()} 渲染成图标+数字）。
        GhostQiCostVarOf(1),
        new DamageVar(8m, ValueProp.Move),
        ModCardVars.Int("Vulnerable", VulnerableAmount)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await ApplyTo<VulnerablePower>(choiceContext, cardPlay.Target,
            DynamicVars.GetIntOrDefault("Vulnerable", VulnerableAmount));
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Vulnerable"].UpgradeValueBy(1);
    }
}
