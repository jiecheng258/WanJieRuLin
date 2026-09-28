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
/// ★ **伏笔**极意：3 回合后，对所有敌人造成 30 点伤害。升级后 45 点。
/// ★ 全模组最高的延迟爆发 —— 要活着等三回合。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ShiRiHou : WanJieRuLinCardModel
{
    public ShiRiHou() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Damage", 30),
        ModCardVars.Int("Turns", 3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<FuBiPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.Damage = DynamicVars.GetIntOrDefault("Damage", 30);
            power.TurnsLeft = DynamicVars.GetIntOrDefault("Turns", 3);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Damage"].UpgradeValueBy(15);    }
}
