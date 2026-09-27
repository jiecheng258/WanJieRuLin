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
/// 蜕鬼 —— 攻击牌 1 能量：**鬼气为 0 时才能打出**。造成 10 点伤害，共 2 次。升级后 3 次。
/// 「蜕去鬼气」的仪式：只有身无鬼气时才能净身上阵。
/// ★ v0.3：这是 L 流（蜕鬼）的**收尾牌** —— 2×4=8 太弱，配不上「清空鬼气」的代价；
///   改为 10×2=20，升级 ★ 加次数 2→3（多段攻击经典升级手法）。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class TuiGui : WanJieRuLinCardModel
{
    private const int BaseHits = 2;

    public TuiGui() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move),
        ModCardVars.Repeat(BaseHits)
    ];

    /// <summary>只有鬼气为 0 时才能打出。</summary>
    protected override bool? PlayCondition => MyGhostQi == 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
    }
}
