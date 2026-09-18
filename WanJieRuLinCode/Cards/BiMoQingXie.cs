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
/// 笔墨倾泻 —— 攻击牌 X 点能量、X 点鬼气：
/// 对所有敌人造成 6 点伤害 X 次，自身获得 X 点（能量 + 鬼气）覆甲。升级后每次伤害 9 点。
///
/// 双 X 费：一次性倾泻全部能量与鬼气，是「画师的全力一击」。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class BiMoQingXie : WanJieRuLinCardModel
{
    /// <summary>
    /// 注意构造函数第一个参数是 <c>0</c>，不是 <c>-1</c>。
    /// X 费不是用负数约定的：必须传 0 + 重写 <see cref="HasEnergyCostX"/>。
    /// </summary>
    public BiMoQingXie() : base(0, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
        SetGhostQiCostX();
    }

    /// <summary>
    /// 声明本牌为 X 能量费用牌。
    ///
    /// 原版 <c>CardEnergyCost</c> 懒构造时会读这个属性，
    /// 只有它为 true，<c>ResolveEnergyXValue()</c> 才会返回实际 X 值而不抛异常。
    /// 原版 Whirlwind / Skewer / MultiCast / Tempest / Malaise 都是这么写的。
    /// </summary>
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var x = EnergyXValue;
        if (x <= 0)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitCount(x)
            .Execute(choiceContext);

        // 覆甲 = 能量 + 鬼气的总投入量。
        var plating = x + GhostQiXValue(cardPlay);
        if (plating > 0)
        {
            await ApplySelf<PlatingPower>(choiceContext, plating);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
