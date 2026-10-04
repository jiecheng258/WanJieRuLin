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
/// ★ **偏激流 · 纯点** 的支撑能力 ——
/// 每当你打出一张「点」牌，获得 1 点能量（每回合上限 2 次）。升级后上限 3 次。
/// ★ 让「只堆点牌」成为一条能赢的路线：点牌本身数值低，但打出它就能换能量。
/// ★ 三张偏激流能力**互斥**，逼你选边。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class RunBi : WanJieRuLinCardModel
{
    public RunBi() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Cap", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<RunBiPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.Cap = DynamicVars.GetIntOrDefault("Cap", 2);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cap"].UpgradeValueBy(1);    }
}
