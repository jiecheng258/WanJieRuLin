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
/// 浓淡相生 —— Skill牌 1费
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class NongDanXiangSheng : WanJieRuLinCardModel
{
    public NongDanXiangSheng() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Swing", 5)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var swing = DynamicVars.GetIntOrDefault("Swing", 5);
        if (IsDenseInk)
        {
            await LoseGhostQi(swing);
            await Draw(choiceContext, 2);
        }
        else
        {
            await GainGhostQi(swing);
            await Draw(choiceContext, 1);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Swing"].UpgradeValueBy(2);    }
}
