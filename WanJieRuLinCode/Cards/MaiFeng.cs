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
/// ★ **伏笔**（v0.4 新机制）：2 回合后，对所有敌人造成 14 点伤害。
/// 升级后 20 点伤害。★ 把「现在」换成「以后」——远期布局的回报。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MaiFeng : WanJieRuLinCardModel
{
    public MaiFeng() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Damage", 14),
        ModCardVars.Int("Turns", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<FuBiPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.Damage = DynamicVars.GetIntOrDefault("Damage", 14);
            power.TurnsLeft = DynamicVars.GetIntOrDefault("Turns", 2);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Damage"].UpgradeValueBy(6);    }
}
