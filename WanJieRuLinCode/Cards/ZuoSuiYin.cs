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
/// 作祟引 —— 抽 2 张牌，获得 1 鬼气，本回合你的作祟费用 -1 鬼气。升级：抽 3，作祟 -2。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ZuoSuiYin : WanJieRuLinCardModel
{
    public ZuoSuiYin() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Draw", 2m),
        GhostQiGainVarOf(1),
        new DynamicVar("CostReduction", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Draw(choiceContext, IsUpgraded ? 3 : 2);
        await GainGhostQi(1);

        // 本回合作祟费用 -N 鬼气（由 ZuoSuiJingPower 承载）
        var power = await ApplySelfAndGet<ZuoSuiJingPower>(choiceContext, 1m);
        if (power is { } p)
        {
            p.CostReduction = IsUpgraded ? 2 : 1;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Draw"].UpgradeValueBy(1m);          // 2 → 3
        DynamicVars["CostReduction"].UpgradeValueBy(1m); // 1 → 2
    }
}
