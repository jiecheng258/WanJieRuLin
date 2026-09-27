using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 耗费所有鬼气。对所有敌人造成 X+4 点伤害。升级后 X+6。
/// ★ 0 费但有鬼气费（R1 只约束「产」资源），只吃不产。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class BiMoQingXie : WanJieRuLinCardModel
{
    public BiMoQingXie() : base(0, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
        SetGhostQiCostX();
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("BonusDamage", 4)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DealDamageToAll(choiceContext, GhostQiXValue(cardPlay) + DynamicVars.GetIntOrDefault("BonusDamage", 4));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BonusDamage"].UpgradeValueBy(2);    }
}
