using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 尸瞳 —— 技能牌 3 鬼气：本回合每打出一张牌，获得 1 点鬼气。升级后鬼气消耗为 2。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ShiTong : WanJieRuLinCardModel
{
    private const int GhostQiCost = 3;

    public ShiTong() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        SetGhostQiCost(GhostQiCost);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 鬼气费用（卡面用 {GhostQiCost:secondaryResourceIcons()} 渲染成图标+数字）。
        GhostQiCostVarOf(3),
        ModCardVars.Int("GhostQiPerCard", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<ShiTongPower>(choiceContext, 1m);
    }

    protected override void OnUpgrade()
    {
        // 鬼气消耗 3 → 2。
        SetGhostQiCost(GhostQiCost - 1);
    }
}
