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
/// 回墨 —— 抽 1 张牌。若本回合已打 2 张以上牌，再抽 1 张。升级：抽 2 张（条件再抽 1）。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class HuiMo : WanJieRuLinCardModel
{
    public HuiMo() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Draw", IsUpgraded ? 2m : 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Draw(choiceContext, IsUpgraded ? 2 : 1);

        // 本回合已打 2 张以上牌（含本张之前），再抽 1 张。
        if (CardsPlayedThisTurn >= 2)
        {
            await Draw(choiceContext, 1);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Draw"].UpgradeValueBy(1m); // 1 → 2
    }
}
