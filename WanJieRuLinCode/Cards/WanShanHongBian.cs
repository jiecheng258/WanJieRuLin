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
/// ★ **面** —— 对**所有**敌人造成 29 点伤害。[消耗]
/// **消耗 5 点墨韵**，改为 36 点。升级后 30 / 42。
/// ★ 细水长流的群体兑现。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class WanShanHongBian : WanJieRuLinCardModel
{
    public WanShanHongBian() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Face;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(29m, ValueProp.Move),
        ModCardVars.Int("MoYunCost", 5),
        ModCardVars.Int("Bonus", 14)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cost = Math.Max(0, DynamicVars.GetIntOrDefault("MoYunCost", 5));
        var boosted = MyMoYun >= cost;
        if (boosted)
        {
            await MoYunPower.Gain(choiceContext, Owner!.Creature, -cost);
        }
        await DealDamageToAll(choiceContext, DynamicVars.Damage.BaseValue + (boosted ? DynamicVars.GetIntOrDefault("Bonus", 12) : 0m));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);    }
}
