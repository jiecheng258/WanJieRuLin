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
/// ★ **相位天气**极意：每回合开始时随机降下一种墨相，强度 8。升级后 12。
/// ★ 长期收益期望高于同费引擎，代价是完全不可控。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MoXiangLiuZhuan : WanJieRuLinCardModel
{
    public MoXiangLiuZhuan() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Amount", 8)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<TianQiPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.Amount = DynamicVars.GetIntOrDefault("Amount", 8);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Amount"].UpgradeValueBy(4);    }
}
