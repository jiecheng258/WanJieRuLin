using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 疏影 —— Skill牌 1费
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ShuYing : WanJieRuLinCardModel
{
    public ShuYing() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("PerQi", 2),
        ModCardVars.Int("Cap", 15)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cap = DynamicVars.GetIntOrDefault("Cap", 15);
        var amount = Math.Min(MyGhostQi * DynamicVars.GetIntOrDefault("PerQi", 2), cap);
        if (amount > 0)
        {
            await GainBlock(choiceContext, amount);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cap"].UpgradeValueBy(5);    }
}
