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
/// 鬼气在 3 到 7 之间时才能打出。本回合每打出一张牌，获得 1 点鬼气。
/// ★ M 流专属硬条件牌 —— 与 H（≥8）、L（≤2）都互斥。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ShiTong : WanJieRuLinCardModel
{
    public ShiTong() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override bool? PlayCondition => GhostQiAtLeast(3) && GhostQiAtMost(7);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<ShiTongPower>(choiceContext, 1m);
    }

    protected override void OnUpgrade()
    {
    }
}
