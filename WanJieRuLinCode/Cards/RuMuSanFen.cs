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
/// 能力 —— **纯线流**：你打出的「线」牌额外 +1 临时力量 +1 临时敏捷。
/// 让线牌的滚雪球翻倍。
/// 
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class RuMuSanFen : WanJieRuLinCardModel
{
    public RuMuSanFen() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<RuMuSanFenPower>(choiceContext, 1m);
        if (power is not null)
        {
        }
    }

    protected override void OnUpgrade()
    {
    }
}
