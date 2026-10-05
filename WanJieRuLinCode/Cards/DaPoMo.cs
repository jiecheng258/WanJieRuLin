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
/// ★ **面** —— 对**所有**敌人造成 24 点伤害。升级后 26 点。
/// ★ 3 费群体 —— 靠笔锋减费打出才是它的正确用法。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class DaPoMo : WanJieRuLinCardModel
{
    public DaPoMo() : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Face;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(24m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DealDamageToAll(choiceContext, DynamicVars.Damage.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);    }
}
