using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 我若为鬼 —— 技能牌 1 能量：**下回合**获得的能量转为鬼气值，下回合抽 2 张牌。消耗。
/// 升级后抽 3 张牌。
///
/// ★ 时长必须显式设成 <b>1 回合</b>：
///   <see cref="EnergyToGhostQiPower"/> 由三张牌共用，默认是**永久**
///   （「厉鬼复苏」「鬼气森森」要的就是永久）。
///   本牌文案写的是「下回合」，所以只有它需要显式改成一次性。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class WoRuoWeiGui : WanJieRuLinCardModel
{
    private const int CardsToDraw = 2;

    public WoRuoWeiGui() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(CardsToDraw)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<EnergyToGhostQiPower>(choiceContext, 1m);
        power?.Configure(1);            // 「下回合」= 只作用一个回合

        await ApplySelf<DrawCardsNextTurnPower>(
            choiceContext, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
