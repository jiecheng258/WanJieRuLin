using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 厉鬼复苏 —— 能力牌 3 能量：接下来的回合获得的能量转为等量鬼气，
/// 消耗掉所有的能量牌，每回合开始发现一张耗费鬼气的牌，它可以免费打出一次。
/// 升级后能量为 2。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class LiGuiFuSu : WanJieRuLinCardModel
{
    /// <summary>每回合开始时额外获得的鬼气。</summary>
    private const int GhostQiPerTurn = 2;

    public LiGuiFuSu() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<EnergyToGhostQiPower>(choiceContext, 1m);
        if (power is not null)
        {
            // 文案写「每回合」→ 永久。
            power.Configure(EnergyToGhostQiPower.Permanent);

            // 光把能量换成鬼气是等价交换（1 鬼气 ≈ 1 能量），没有净收益；
            // 补一份每回合的额外鬼气，这张 3 费稀有才有存在意义。
            power.GhostQiPerTurn = GhostQiPerTurn;
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
