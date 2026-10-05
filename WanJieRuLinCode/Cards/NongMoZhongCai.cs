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
/// ★ **面** —— 造成 13 点伤害；**消耗 3 点[gold]墨韵[/gold]**，此牌伤害 +9。升级后 13 点。
/// ★ 墨韵的兑现口 —— 囤够了就打。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class NongMoZhongCai : WanJieRuLinCardModel
{
    public NongMoZhongCai() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Face;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(13m, ValueProp.Move),
        ModCardVars.Int("MoYunCost", 3),
        ModCardVars.Int("Bonus", 11)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cost = Math.Max(0, DynamicVars.GetIntOrDefault("MoYunCost", 3));
        if (cost > 0 && MyMoYun >= cost)
        {
            await MoYunPower.Gain(choiceContext, Owner!.Creature, -cost);
        }
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + (MyMoYun >= cost ? DynamicVars.GetIntOrDefault("Bonus", 9) : 0m))
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);    }
}
