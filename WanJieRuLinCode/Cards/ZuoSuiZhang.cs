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
/// 作祟掌 —— 作祟影响张数 +2。升级：+3。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ZuoSuiZhang : WanJieRuLinCardModel
{
    public ZuoSuiZhang() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("ExtraCount", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<ZuoSuiZhangPower>(choiceContext, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ExtraCount"].UpgradeValueBy(1m); // 2 → 3
    }
}
