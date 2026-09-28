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
/// 每回合最多触发 1 次：失去生命时，每失去 2 点生命获得 1 点鬼气。
/// 升级后上限 2 次。★ 反无限（R5），且与「血墨」自伤牌天然联动。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class GuangMingYuYan : WanJieRuLinCardModel
{
    public GuangMingYuYan() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("MaxTriggersPerTurn", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<GuangMingYuYanPower>(choiceContext, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxTriggersPerTurn"].UpgradeValueBy(1);    }
}
