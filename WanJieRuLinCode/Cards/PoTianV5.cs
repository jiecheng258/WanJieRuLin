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
/// ★ **面** —— **消耗所有[gold]墨韵[/gold]**，每点对所有敌人造成 3 点伤害。[消耗]
/// 升级后每点 4 点。★ 墨韵越厚越恐怖 —— 细水长流的终极兑现。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class PoTianV5 : WanJieRuLinCardModel
{
    public PoTianV5() : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Face;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("PerMoYun", 4)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var mo = MyMoYun;
        if (mo > 0)
        {
            await MoYunPower.Gain(choiceContext, Owner!.Creature, -mo);
        }
        if (mo > 0)
        {
            await DealDamageToAll(choiceContext, mo * DynamicVars.GetIntOrDefault("PerMoYun", 3));
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PerMoYun"].UpgradeValueBy(1);    }
}
