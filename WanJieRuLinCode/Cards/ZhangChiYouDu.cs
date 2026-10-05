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
using WanJieRuLin.Aspects;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 能力 —— 你打出「点」牌时，抽 1 张牌（每回合上限 2 次）。
/// 升级 ★ 效果升级：上限 2 → 3。
/// ★ 三类联系：点牌便宜，产出的是手牌流量。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ZhangChiYouDu : WanJieRuLinCardModel
{
    public ZhangChiYouDu() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Cap", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<ZhangChiYouDuPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.Cap = DynamicVars.GetIntOrDefault("Cap", 2);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cap"].UpgradeValueBy(1);    }
}
