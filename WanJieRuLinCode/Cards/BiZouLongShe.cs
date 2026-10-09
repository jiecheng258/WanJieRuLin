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
/// 能力 —— **纯点流**：每回合开始时额外获得 1 点能量。
/// 让点牌更便宜，鼓励以点牌起手。
/// 
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class BiZouLongShe : WanJieRuLinCardModel
{
    public BiZouLongShe() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<BiZouLongShePower>(choiceContext, 1m);
        if (power is not null)
        {
        }
    }

    protected override void OnUpgrade()
    {
    }
}
