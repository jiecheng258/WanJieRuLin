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
/// 盲心剑 —— 攻击牌 1 能量：造成 8 点伤害，降低 8 点力量。消耗。升级后造成 12 点伤害。
///
/// 数值说明（对齐原版）：原版最接近的是「刺耳尖啸」（普通，1 费，对**所有**敌人 -6 力量，
/// 不造成伤害）。本卡是单体，所以减力量给到 8（比 AoE 版高），
/// 但直伤要从 10 压到 8 —— 原来「10 伤 + 减 10 力量」相当于
/// 把两个 1 费效果塞进 1 费里，即使有「消耗」也超模。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MangXinJian : WanJieRuLinCardModel
{
    private const int StrengthLoss = 8;
    private const int BaseDamage = 8;

    public MangXinJian() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        ModCardVars.Power<StrengthPower>(-StrengthLoss)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var target = cardPlay.Target;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .Execute(choiceContext);

        await ApplyTo<StrengthPower>(choiceContext, target, -StrengthLoss);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
