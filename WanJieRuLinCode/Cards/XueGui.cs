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
/// 血鬼 —— 失去 4 生命，造成 18 伤害。升级：失去 3 生命，造成 22 伤害。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class XueGui : WanJieRuLinCardModel
{
    public XueGui() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HpLoss", 4m),
        new DamageVar(18m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        // 自伤：失去生命（无法格挡的真实伤害打自己）
        if (Owner is { } p)
        {
            await CreatureCmd.Damage(
                choiceContext, p.Creature,
                IsUpgraded ? 3m : 4m,
                ValueProp.Unblockable,
                p.Creature, this, cardPlay);
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HpLoss"].UpgradeValueBy(-1m);   // 4 → 3
        DynamicVars.Damage.UpgradeValueBy(4m);        // 18 → 22
    }
}
