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
/// 灵虚 —— 本回合你所有 0 费牌可免费再打出一次（其复制入弃牌堆）。升级：所有 1 费牌也可。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class LingXu : WanJieRuLinCardModel
{
    public LingXu() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("CostThreshold", 0m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<LingXuPower>(choiceContext, 1m);
        if (power is { } p)
        {
            p.IsUpgraded = IsUpgraded;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["CostThreshold"].UpgradeValueBy(1m); // 0 → 1
    }
}
