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
/// ★ **偏激流 · 纯面** 的支撑能力 ——
/// [gold]墨韵[/gold]**不再削弱「点 / 线」牌**，改为每满 5 层，你的「面」牌伤害 +3。
/// 升级后 +5。
/// ★ 把墨韵从「双刃」变成「纯增益」——「只堆面牌」成为越打越强的路线。
/// ★ 代价：你彻底放弃点/线，前期会很难受。三张偏激流能力**互斥**。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class PoTianAbility : WanJieRuLinCardModel
{
    public PoTianAbility() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("PerStep", 4)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<PoTianPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.PerStep = DynamicVars.GetIntOrDefault("PerStep", 3);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PerStep"].UpgradeValueBy(2);    }
}
