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
/// ★ **面** —— 对**所有**敌人造成 14 点伤害。升级后 16 点。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class PoMoV5 : WanJieRuLinCardModel
{
    public PoMoV5() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Face;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(14m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DealDamageToAll(choiceContext, DynamicVars.Damage.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);    }
}
