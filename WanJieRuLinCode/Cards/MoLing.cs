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
/// 墨灵 —— 每当你打出作祟，获得 1 力量（每回合上限 3）。升级：上限 5。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MoLing : WanJieRuLinCardModel
{
    public MoLing() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Strength", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<MoLingPower>(choiceContext, 1m);
        if (power is { } p)
        {
            p.IsUpgraded = IsUpgraded;
        }
    }

    protected override void OnUpgrade()
    {
        // 升级 = 上限 5
    }
}
