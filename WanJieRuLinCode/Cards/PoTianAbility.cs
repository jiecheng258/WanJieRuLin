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
/// 能力 —— **纯面流**：你的「面」牌伤害额外 +3。
/// 升级 ★ 效果升级：+5。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class PoTianAbility : WanJieRuLinCardModel
{
    public PoTianAbility() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Bonus", 3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<PoTianPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.PerStep = DynamicVars.GetIntOrDefault("Bonus", 3);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Bonus"].UpgradeValueBy(2);    }
}
