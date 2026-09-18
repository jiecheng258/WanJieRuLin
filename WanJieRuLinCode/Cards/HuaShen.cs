using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 化神 —— 能力牌 3 能量：每消耗 1 点鬼气，抽 1 张牌并获得 1 点能量。升级后能量消耗为 2。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class HuaShen : WanJieRuLinCardModel
{
    public HuaShen() : base(3, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<HuaShenPower>(choiceContext, 1);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
