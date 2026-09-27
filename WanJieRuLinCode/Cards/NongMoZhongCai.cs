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
/// 每回合最多获得 12 点此牌格挡：你获得鬼气时，获得等量的格挡。升级后上限 18。
/// ★ H 流引擎：产气即叠甲。有每回合上限。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class NongMoZhongCai : WanJieRuLinCardModel
{
    public NongMoZhongCai() : base(3, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("MaxBlockPerTurn", 12)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<RichInkPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.MaxBlockPerTurn = DynamicVars.GetIntOrDefault("MaxBlockPerTurn", 12);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxBlockPerTurn"].UpgradeValueBy(6);    }
}
