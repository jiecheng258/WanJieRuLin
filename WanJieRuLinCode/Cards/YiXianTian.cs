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
/// 线 —— **需本回合已打出 3 张以上线牌**才能打出。
/// 造成 24 点伤害。
/// 升级后 30 点伤害。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class YiXianTian : WanJieRuLinCardModel
{
    public YiXianTian() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Line;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(24m, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override bool? PlayCondition => QianJunAtLeast(3);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await LineHit(choiceContext);
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);    }
}
