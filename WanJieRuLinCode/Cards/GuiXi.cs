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
/// 鬼息 —— 每当你获得鬼气，获得 2 格挡。升级：3 格挡。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class GuiXi : WanJieRuLinCardModel
{
    public GuiXi() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Block", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<GuiXiPower>(choiceContext, 1m);
        if (power is { } p)
        {
            p.IsUpgraded = IsUpgraded;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(1m); // 2 → 3
    }
}
