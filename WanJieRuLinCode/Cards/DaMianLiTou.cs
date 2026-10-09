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
/// 能力 —— **纯面流**：你的「面」牌回流额外 +1 次（每场每张可回流 2 次）。
/// 面牌总收益翻倍，是面流后期爆发的关键件。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class DaMianLiTou : WanJieRuLinCardModel
{
    public DaMianLiTou() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<DaMianLiTouPower>(choiceContext, 1m);
        if (power is not null)
        {
        }
    }

    protected override void OnUpgrade()
    {
    }
}
