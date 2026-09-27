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
/// 每回合最多触发 2 次：获得鬼气时获得 3 点格挡；失去鬼气时抽 1 张牌。
/// 升级后上限 3 次。
/// ★ 反无限（R5）：原为无上限触发，与产气牌直接构成无限。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class NongDanYouXin : WanJieRuLinCardModel
{
    public NongDanYouXin() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("MaxTriggersPerTurn", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<InkBalancePower>(choiceContext, 1m);
        if (power is not null)
        {
            power.MaxTriggersPerTurn = DynamicVars.GetIntOrDefault("MaxTriggersPerTurn", 2);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxTriggersPerTurn"].UpgradeValueBy(1);    }
}
