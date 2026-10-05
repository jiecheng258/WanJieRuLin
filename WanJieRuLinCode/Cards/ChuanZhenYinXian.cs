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
/// 线 —— 造成 7 点伤害。你每打出过 1 张线牌，此伤害 +2。
/// 升级 ★ 效果升级：每张 +2 → +3。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ChuanZhenYinXian : WanJieRuLinCardModel
{
    public ChuanZhenYinXian() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Line;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        ModCardVars.Int("PerLine", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await LineHit(choiceContext);
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DealDamage(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue + MyQianJun * DynamicVars.GetIntOrDefault("PerLine", 2));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PerLine"].UpgradeValueBy(1);    }
}
