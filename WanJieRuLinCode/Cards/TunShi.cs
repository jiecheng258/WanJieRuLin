using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 吞噬 —— 能力牌 2 能量：每获得 1 点鬼气，抽 1 张牌。升级后能量消耗为 1。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class TunShi : WanJieRuLinCardModel
{
    public TunShi() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<TunShiPower>(choiceContext, 1);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
