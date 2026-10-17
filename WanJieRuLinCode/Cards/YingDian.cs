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
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 影点 —— 造成 1 点伤害，重复「当前鬼气」次（至多 8 次）。升级：至多 12 次。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class YingDian : WanJieRuLinCardModel
{
    public YingDian() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(1m, ValueProp.Move),
        new DynamicVar("Cap", IsUpgraded ? 12m : 8m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var cap = IsUpgraded ? 12 : 8;
        var hits = Math.Min(cap, MyGhostQi);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitCount(hits)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cap"].UpgradeValueBy(4m); // 8 → 12
    }
}
