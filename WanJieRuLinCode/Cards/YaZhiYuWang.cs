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
/// 耗费 1 点鬼气。下回合开始时获得 3 点鬼气。升级后 5 点。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class YaZhiYuWang : WanJieRuLinCardModel
{
    public YaZhiYuWang() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        SetGhostQiCost(1);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("GhostQiCost", 1),
        ModCardVars.Int("NextTurn", 3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<GhostQiNextTurnPower>(choiceContext, DynamicVars.GetIntOrDefault("NextTurn", 3));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["NextTurn"].UpgradeValueBy(2);    }
}
