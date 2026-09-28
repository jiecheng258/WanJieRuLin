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
/// 耗费所有鬼气。对所有敌人造成 8 点伤害，每耗费 1 点鬼气此伤害 +3。
/// 升级后基础 12 点。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class XianMian : WanJieRuLinCardModel
{
    public XianMian() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
        SetGhostQiCostX();
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
        ModCardVars.Int("BonusPerQi", 3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DealDamageToAll(choiceContext, DynamicVars.Damage.BaseValue + GhostQiXValue(cardPlay) * DynamicVars.GetIntOrDefault("BonusPerQi", 3));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);    }
}
