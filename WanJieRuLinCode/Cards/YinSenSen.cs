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
/// 阴森森 —— 阴森洗入牌库的衍生物：0 能量，造成 9 点伤害 3 次。
/// 不进卡池、不进图鉴（Token）。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class YinSenSen : WanJieRuLinCardModel
{
    private const int Hits = 3;

    public YinSenSen() : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, shouldShowInCardLibrary: false)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9m, ValueProp.Move),
        ModCardVars.Repeat(Hits)
    ];

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
        // 衍生物不可升级。
    }
}
