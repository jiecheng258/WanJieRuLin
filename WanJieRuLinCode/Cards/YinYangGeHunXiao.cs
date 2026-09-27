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
/// 耗费 2 点鬼气。打出攻击牌时抽 1 张；打出技能牌时获得 1 点格挡。
/// 升级质变：每次抽 1→2 张。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class YinYangGeHunXiao : WanJieRuLinCardModel
{
    public YinYangGeHunXiao() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        SetGhostQiCost(2);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("GhostQiCost", 2),
        ModCardVars.Int("DrawPerAttack", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<YinYangGeHunXiaoPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.DrawPerAttack = DynamicVars.GetIntOrDefault("DrawPerAttack", 1);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DrawPerAttack"].UpgradeValueBy(1);    }
}
