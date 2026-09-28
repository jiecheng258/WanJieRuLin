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
/// ★ **相位天气**（v0.4 新机制）：每回合开始时随机降下一种墨相 ——
/// 厚（获得 5 点格挡）／润（获得 5 点鬼气）／活（获得 1 点能量）。
/// 升级后强度 8。★ 不确定但高收益，考验即时决策。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class TianShi : WanJieRuLinCardModel
{
    public TianShi() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Amount", 5)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<TianQiPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.Amount = DynamicVars.GetIntOrDefault("Amount", 5);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Amount"].UpgradeValueBy(3);    }
}
