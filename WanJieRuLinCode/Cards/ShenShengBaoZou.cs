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
/// 本回合你的攻击牌伤害翻倍；每打出 3 张攻击牌，额外消耗 1 点鬼气
/// （鬼气不足时该牌无法打出）。升级后每 4 张。
/// ★ R8：单回合增幅。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ShenShengBaoZou : WanJieRuLinCardModel
{
    public ShenShengBaoZou() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("AttacksPerGhostQi", 3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<DivineRampagePower>(choiceContext, 1m);
        if (power is not null)
        {
            power.AttacksPerGhostQi = DynamicVars.GetIntOrDefault("AttacksPerGhostQi", 3);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AttacksPerGhostQi"].UpgradeValueBy(1);    }
}
