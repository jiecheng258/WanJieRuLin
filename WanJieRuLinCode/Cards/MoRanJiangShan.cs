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
/// 耗费所有鬼气。本回合获得 X+3 点临时力量与 X+3 点临时敏捷（X 为耗费的鬼气）。
/// [gold]保留[/gold]。★ 先古牌：不抽牌不产能量，纯增幅。
/// </summary>
[RegisterDustyTomeCard(typeof(WanJieRuLinCharacter))]
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MoRanJiangShan : WanJieRuLinCardModel
{
    public MoRanJiangShan() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
        SetGhostQiCostX();
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("BonusStat", 3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var x = GhostQiXValue(cardPlay);
        var amt = x + DynamicVars.GetIntOrDefault("BonusStat", 3);
        await ApplySelf<MoRanJiangShanTempStrengthPower>(choiceContext, amt);
        await ApplySelf<MoRanJiangShanTempDexterityPower>(choiceContext, amt);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BonusStat"].UpgradeValueBy(3);    }
}
