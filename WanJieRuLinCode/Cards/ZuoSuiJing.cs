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
/// 作祟精 —— 你的作祟费用减半（向下取整）。升级：作祟费用 -3 鬼气。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ZuoSuiJing : WanJieRuLinCardModel
{
    public ZuoSuiJing() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("CostReduction", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<ZuoSuiJingPower>(choiceContext, 1m);
        if (power is { } p)
        {
            // 作祟精：减半向下取整（由 ZuoSui 读取时处理），升级则 -3 鬼气
            p.CostReduction = IsUpgraded ? 3 : 0;
        }
    }

    protected override void OnUpgrade()
    {
        // 升级 = 作祟费用 -3 鬼气（而非减半）
    }
}
