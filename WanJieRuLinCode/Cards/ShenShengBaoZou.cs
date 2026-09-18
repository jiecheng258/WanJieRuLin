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
/// 神圣暴走 —— 技能牌 2 鬼气：本回合攻击伤害翻倍，
/// 但每打出 1 张攻击牌消耗 1 点鬼气。升级后每打出 2 张攻击牌消耗 1 点鬼气。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ShenShengBaoZou : WanJieRuLinCardModel
{
    private const int GhostQiCost = 2;
    private const int BaseAttacksPerGhostQi = 1;

    public ShenShengBaoZou() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        SetGhostQiCost(GhostQiCost);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 鬼气费用（卡面用 {GhostQiCost:secondaryResourceIcons()} 渲染成图标+数字）。
        GhostQiCostVarOf(2),
        ModCardVars.Int("AttacksPerGhostQi", BaseAttacksPerGhostQi)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<DivineRampagePower>(choiceContext, 1m);
        if (power is not null)
        {
            power.AttacksPerGhostQi =
                DynamicVars.GetIntOrDefault("AttacksPerGhostQi", BaseAttacksPerGhostQi);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AttacksPerGhostQi"].UpgradeValueBy(1);
    }
}
