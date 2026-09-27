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
/// 获得 2 点鬼气；下回合开始时再获得 2 点。升级后立即获得 3 点。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class NingMo : WanJieRuLinCardModel
{
    public NingMo() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("GhostQiGain", 2),
        ModCardVars.Int("NextTurn", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainGhostQi(DynamicVars.GetIntOrDefault("GhostQiGain", 2));
        await ApplySelf<GhostQiNextTurnPower>(choiceContext, DynamicVars.GetIntOrDefault("NextTurn", 2));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["GhostQiGain"].UpgradeValueBy(1);    }
}
