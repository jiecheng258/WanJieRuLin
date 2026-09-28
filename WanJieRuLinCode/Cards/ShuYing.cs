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
/// 获得等于当前鬼气 2 倍的格挡（最多 14 点）。升级后上限 20 点。
/// ★ 单次结算，无增量循环。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ShuYing : WanJieRuLinCardModel
{
    public ShuYing() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("PerQi", 2),
        ModCardVars.Int("Cap", 14)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var raw = MyGhostQi * DynamicVars.GetIntOrDefault("PerQi", 2);
        var cap = DynamicVars.GetIntOrDefault("Cap", 14);
        await GainBlock(choiceContext, Math.Min(raw, cap));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cap"].UpgradeValueBy(6);    }
}
