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
/// 失去所有鬼气。造成 8 点伤害，每失去 3 点鬼气此伤害 +5。
/// 升级质变：每 2 点即可换一次。★ 单向清空，不给资源。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class XiJinQianHua : WanJieRuLinCardModel
{
    public XiJinQianHua() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
        ModCardVars.Int("QiPerBonus", 3),
        ModCardVars.Int("BonusPer", 5)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var spent = await ClearGhostQi();
        var per = Math.Max(1, DynamicVars.GetIntOrDefault("QiPerBonus", 3));
        var bonus = (spent / per) * DynamicVars.GetIntOrDefault("BonusPer", 5);
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["QiPerBonus"].UpgradeValueBy(-1);    }
}
