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
/// 点，线 —— 攻击牌 0 能量、耗费全部鬼气：造成 6 + 每点鬼气 3 点伤害。
/// 升级后基础伤害 9。
///
/// 「点，线」是画面生成的基础：以鬼气为墨，一笔到底。
///
/// 数值说明（对齐原版）：本模组自己的汇率是「1 鬼气 ≈ 1 能量」
/// （见「审时度势」1 鬼气换 1 能量），而原版 1 能量约等于 6-10 点单体伤害。
/// 原来每点鬼气只换 +1 伤害，导致「花光一管鬼气换十几点伤害」——
/// 同样花 3 点鬼气的「幽然鬼火」是 18 伤 + 4 易伤，本卡严重偏弱。
/// 现改为每点鬼气 +3（基础 6），至少不再是个陷阱选项。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class DianXian : WanJieRuLinCardModel
{
    private const int BaseDamage = 6;
    private const int DamagePerGhostQi = 3;

    public DianXian() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        SetGhostQiCostX();
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var x = GhostQiXValue(cardPlay);
        var total = DynamicVars.Damage.BaseValue + x * DamagePerGhostQi;

        await DamageCmd.Attack(total)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
