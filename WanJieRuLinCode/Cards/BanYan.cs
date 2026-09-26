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
/// 半砚 —— Power牌 1费
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class BanYan : WanJieRuLinCardModel
{
    public BanYan() : base(1, CardType.Power, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("BlockPerTurn", 4)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<HalfInkPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.BlockPerTurn = DynamicVars.GetIntOrDefault("BlockPerTurn", 4);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlockPerTurn"].UpgradeValueBy(2);    }
}
