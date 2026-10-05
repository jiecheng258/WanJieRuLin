using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Aspects;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 点 —— 0 费：造成 4 点伤害。你每打出过 1 张点牌，此伤害 +2。
/// 升级 ★ 效果升级：每张 +2 → +3。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class HaoLi : WanJieRuLinCardModel
{
    public HaoLi() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Point;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move),
        ModCardVars.Int("PerDian", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PointHit(choiceContext);
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DealDamage(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue + MyLuanDian * DynamicVars.GetIntOrDefault("PerDian", 2));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PerDian"].UpgradeValueBy(1);    }
}
