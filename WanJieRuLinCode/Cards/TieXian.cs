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
/// 线 —— 造成 10 点伤害。你每打出过 1 张线牌，此伤害 +3。
/// 升级 ★ 效果升级：每张 +3 → +4。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class TieXian : WanJieRuLinCardModel
{
    public TieXian() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Line;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move),
        ModCardVars.Int("PerLine", 3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await LineHit(choiceContext);
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DealDamage(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue + MyQianJun * DynamicVars.GetIntOrDefault("PerLine", 3));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PerLine"].UpgradeValueBy(1);    }
}
