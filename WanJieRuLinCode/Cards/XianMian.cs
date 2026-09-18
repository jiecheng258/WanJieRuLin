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
/// 线，面 —— 攻击牌 1 能量、耗费全部鬼气：
/// 对所有敌人造成 X 点伤害，共 3 次（X 为你耗费的鬼气）。
/// 升级后伤害 +2、次数 3 → 4。
///
/// 卡面显示注意：「耗费的鬼气」是运行期才知道的值，**不能**用一个固定变量去占位
/// （原来这里放了一个名为 GhostQiGain、值为 1 的变量当公式的第一项，
/// 卡面就会显示成「造成 1+0 点伤害」，与实际结算的「X+0」不符）。
/// 这种运行期数值一律在文案里写字面量 X，和「点，线」「绘」保持一致。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class XianMian : WanJieRuLinCardModel
{
    private const int BaseHits = 3;
    private const int UpgradeHits = 4;
    private const int UpgradeBonus = 2;

    public XianMian() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
        SetGhostQiCostX();
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Bonus", 0),
        ModCardVars.Repeat(BaseHits)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ghostQi = GhostQiXValue(cardPlay);
        var damage = ghostQi + DynamicVars.GetIntOrDefault("Bonus", 0);
        if (damage <= 0)
        {
            return;
        }

        var hits = DynamicVars.Repeat.IntValue;

        foreach (var enemy in CombatState!.HittableEnemies.ToList())
        {
            await DamageCmd.Attack(damage)
                .FromCard(this, cardPlay)
                .Targeting(enemy)
                .WithHitCount(hits)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Bonus"].UpgradeValueBy(UpgradeBonus);
        DynamicVars.Repeat.UpgradeValueBy(UpgradeHits - BaseHits);
    }
}
