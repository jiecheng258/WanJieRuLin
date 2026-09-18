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
/// 研磨下笔 —— 攻击牌 1 能量：造成 12 点伤害。鬼气值 &gt; 3 时才可打出。
/// 升级后造成 16 点伤害。
///
/// 数值说明（对齐原版）：原版 1 费单体伤害的常见区间是 6-10
/// （打击 6、撞头 9、串刺 5×2），2 费才到 15-18（掠食者 15、煤渣 18）。
/// 本卡虽然有「鬼气 &gt; 3」的门槛，但门槛只是限制，不该换来 2 费级的伤害。
/// 改成 12 后：仍高于同费原版（门槛的溢价），但不再越级。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class YanMoXiaBi : WanJieRuLinCardModel
{
    private const int RequiredGhostQi = 3;
    private const int BaseDamage = 12;

    public YanMoXiaBi() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move)
    ];

    /// <summary>鬼气值大于 3 时才可打出。</summary>
    protected override bool? PlayCondition => MyGhostQi > RequiredGhostQi;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

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
