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
/// 每回合最多触发 5 次：你造成伤害时获得 1 点鬼气。升级后 ★ 质变：上限 5→7。
/// ★ H 流引擎：把攻击变成鬼气，有每回合上限所以不会无限。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class GuiYingSenSen : WanJieRuLinCardModel
{
    public GuiYingSenSen() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Threshold", 5)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<GuiYingSenSenPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.Threshold = DynamicVars.GetIntOrDefault("Threshold", 5);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Threshold"].UpgradeValueBy(-2);    }
}
