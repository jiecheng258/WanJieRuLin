using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 阳光普照 —— 攻击牌 1 能量：造成 1 点伤害，击晕所有敌人。消耗。
/// 鬼气为 0 时才可打出。升级后造成 10 点伤害。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class YangGuangPuZhao : WanJieRuLinCardModel
{
    public YangGuangPuZhao() : base(1, CardType.Attack, CardRarity.Rare, TargetType.None)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(1m, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    /// <summary>鬼气为 0 时才可打出。</summary>
    protected override bool? PlayCondition => MyGhostQi == 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var enemy in CombatState!.HittableEnemies.ToList())
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(enemy)
                .Execute(choiceContext);

            await CreatureCmd.Stun(enemy);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(9m);
    }
}
