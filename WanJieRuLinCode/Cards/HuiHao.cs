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
/// ★ **连笔**（v0.4 新机制）：本回合内，你每打出一张牌，本回合伤害 +2
/// （最多 4 层）。升级后上限 6 层。
/// ★ 逼玩家「一口气把牌打完」，与「留牌过回合」形成取舍。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class HuiHao : WanJieRuLinCardModel
{
    public HuiHao() : base(1, CardType.Power, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("BonusPerStep", 2),
        ModCardVars.Int("MaxSteps", 4)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<LianBiPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.BonusPerStep = DynamicVars.GetIntOrDefault("BonusPerStep", 2);
            power.MaxSteps = DynamicVars.GetIntOrDefault("MaxSteps", 4);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxSteps"].UpgradeValueBy(2);    }
}
