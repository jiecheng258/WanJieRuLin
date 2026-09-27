using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
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
/// 失去所有鬼气，每失去 1 点获得 3 点格挡。升级后每点 4 点。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class XiBi : WanJieRuLinCardModel
{
    public XiBi() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("BlockPerQi", 3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var spent = await ClearGhostQi();
        if (spent > 0)
        {
            await GainBlock(choiceContext, spent * DynamicVars.GetIntOrDefault("BlockPerQi", 3));
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlockPerQi"].UpgradeValueBy(1);    }
}
