using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 层层侵蚀 —— 技能牌 3 鬼气：获得 15 点格挡。若打出此牌后鬼气值为 0，获得 2 点鬼气。
/// 升级后获得 20 点格挡。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class CengCengQinShi : WanJieRuLinCardModel
{
    private const int GhostQiCost = 3;
    private const int RefundGhostQi = 2;

    public CengCengQinShi() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        SetGhostQiCost(GhostQiCost);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 鬼气费用（卡面用 {GhostQiCost:secondaryResourceIcons()} 渲染成图标+数字）。
        GhostQiCostVarOf(3),
        new BlockVar(15m, ValueProp.Move),
        GhostQiGainVarOf(RefundGhostQi)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlock(choiceContext, DynamicVars.Block.BaseValue);

        // 消耗 3 点鬼气后若恰好归零，返还 2 点。
        if (MyGhostQi == 0)
        {
            await GainGhostQi(RefundGhostQi);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(5m);
    }
}
