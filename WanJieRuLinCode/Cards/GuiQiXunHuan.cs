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
/// 每回合结束时：若鬼气不低于 8，失去 2 点；若不高于 2，获得 2 点。升级后 3 点。
/// ★ M 流引擎：把鬼气往中间拉。有每回合上限（R5）。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class GuiQiXunHuan : WanJieRuLinCardModel
{
    public GuiQiXunHuan() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("AmountPerTurn", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<InkCyclePower>(choiceContext, 1m);
        if (power is not null)
        {
            power.AmountPerTurn = DynamicVars.GetIntOrDefault("AmountPerTurn", 2);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AmountPerTurn"].UpgradeValueBy(1);    }
}
