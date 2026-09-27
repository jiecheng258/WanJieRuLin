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
/// 每回合最多触发 2 次：消耗鬼气时，抽 1 张牌并获得 1 点能量。升级后上限 3 次。
/// ★ 反无限（R5）：原为「每消耗 1 点就 ×」，无上限 → 配产气牌即无限。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class HuaShen : WanJieRuLinCardModel
{
    public HuaShen() : base(3, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Cap", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<HuaShenPower>(choiceContext, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cap"].UpgradeValueBy(1);    }
}
