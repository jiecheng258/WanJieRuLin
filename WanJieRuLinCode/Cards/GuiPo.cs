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
/// 鬼魄 —— 每场战斗开始，你获得 3 鬼气。升级：4 鬼气。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class GuiPo : WanJieRuLinCardModel
{
    public GuiPo() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        GhostQiGainVarOf(3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<GuiPoPower>(choiceContext, 1m);
        if (power is { } p)
        {
            p.IsUpgraded = IsUpgraded;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[GhostQiGainVar].UpgradeValueBy(1m); // 3 → 4
    }
}
