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
/// ★ **连笔**极意：本回合每打出一张牌，本回合伤害 +3（最多 5 层）。
/// 升级后上限 8 层。
/// ★ 全模组最高的单回合爆发天花板 —— 前提是你真的能一口气打光手牌。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ZouBiLongShe : WanJieRuLinCardModel
{
    public ZouBiLongShe() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("BonusPerStep", 3),
        ModCardVars.Int("MaxSteps", 5)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<LianBiPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.BonusPerStep = DynamicVars.GetIntOrDefault("BonusPerStep", 3);
            power.MaxSteps = DynamicVars.GetIntOrDefault("MaxSteps", 5);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxSteps"].UpgradeValueBy(3);    }
}
