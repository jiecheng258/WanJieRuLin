using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 傲慢 —— 攻击牌 2 鬼气：造成 8 点伤害。
/// 本回合内，你每次造成伤害都获得等量鬼气。升级后造成 12 点伤害。
///
/// 数值说明（对齐原版）：
/// 鬼气在本模组里约等于 1 点能量（「审时度势」就是 1 鬼气换 1 能量），
/// 所以 2 鬼气 ≈ 2 能量；原版 2 费单体伤害约 15-18，本卡的真身是
/// 「本回合伤害转鬼气」这个能力，所以直伤给到 8（2 鬼气里的一半是能力费）。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class AoMan : WanJieRuLinCardModel
{
    private const int GhostQiCost = 2;
    private const int BaseDamage = 8;

    public AoMan() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        SetGhostQiCost(GhostQiCost);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 鬼气费用（卡面用 {GhostQiCost:secondaryResourceIcons()} 渲染成图标+数字）。
        GhostQiCostVarOf(GhostQiCost),
        new DamageVar(BaseDamage, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await ApplySelf<AoManPower>(choiceContext, 1m);

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
