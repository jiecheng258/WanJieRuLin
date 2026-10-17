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
/// 先古之兆 —— 消耗。获得 5 鬼气，抽 1 张牌。升级：8 鬼气，抽 2 张。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class XianGuZhiZhao : WanJieRuLinCardModel
{
    public XianGuZhiZhao() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        GhostQiGainVarOf(5),
        new DynamicVar("Draw", 1m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainGhostQi(IsUpgraded ? 8 : 5);
        await Draw(choiceContext, IsUpgraded ? 2 : 1);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[GhostQiGainVar].UpgradeValueBy(3m); // 5 → 8
        DynamicVars["Draw"].UpgradeValueBy(1m);          // 1 → 2
    }
}
