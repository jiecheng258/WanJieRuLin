using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 光明预言 —— 能力牌 3 能量：每消耗 1 点鬼气，扣除 2 点生命并发现 1 张无色牌，
/// 在本回合可以免费打。升级后能量为 2。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class GuangMingYuYan : WanJieRuLinCardModel
{
    public GuangMingYuYan() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("HpPerGhostQi", GuangMingYuYanPower.HpLostPerGhostQi)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<GuangMingYuYanPower>(choiceContext, 1m);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
