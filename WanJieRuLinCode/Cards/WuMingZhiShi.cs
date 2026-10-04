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
/// ★ **先古牌** —— 将你的[gold]墨韵[/gold]、[gold]笔锋[/gold]、[gold]力道[/gold]**全部清零**，每清 2 层获得 1 点能量。[消耗]
/// ★ 重置按钮：墨韵堆太高拖累了点/线时，用它换一波费用重新开始。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class WuMingZhiShi : WanJieRuLinCardModel
{
    public WuMingZhiShi() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("PerTwo", 1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var edge = await ClearBiFeng(choiceContext);
        var force = await ClearLiDao(choiceContext);
        var mo = MyMoYun;
        if (mo > 0)
        {
            await MoYunPower.Gain(choiceContext, Owner!.Creature, -mo);
        }
        var total = edge + force + mo;
        var gain = total / 2;
        if (gain > 0)
        {
            await GainEnergy(gain);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
