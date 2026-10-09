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
/// 能力 —— **通用过牌**：每打出 1 张牌，抽 1 张（每回合上限 3 次）。
/// 让手牌流动起来。
/// 
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class BiDuanYiLian : WanJieRuLinCardModel
{
    public BiDuanYiLian() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<BiDuanYiLianPower>(choiceContext, 1m);
        if (power is not null)
        {
        }
    }

    protected override void OnUpgrade()
    {
    }
}
