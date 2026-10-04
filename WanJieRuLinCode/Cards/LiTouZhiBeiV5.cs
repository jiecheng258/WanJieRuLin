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
/// ★ **偏激流 · 纯线** 的支撑能力 ——
/// 每回合开始时获得 2 点[gold]力道[/gold]。升级后 3 点。
/// ★ 让线流有一条稳定底盘：力道越厚，线牌越强。
/// ★ 三张偏激流能力**互斥**。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class LiTouZhiBeiV5 : WanJieRuLinCardModel
{
    public LiTouZhiBeiV5() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Force", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<LiTouPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.Force = DynamicVars.GetIntOrDefault("Force", 2);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Force"].UpgradeValueBy(1);    }
}
