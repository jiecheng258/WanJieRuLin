using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace WanJieRuLin.Cards;

/// <summary>
/// 阴阳割昏晓 —— 能力牌 2 能量、2 鬼气：
/// 每打出 1 张攻击牌抽 1 张牌；每打出 1 张技能牌随机消耗 1 张手牌。
/// 升级后：每张攻击牌抽 2 张牌，技能牌改为「指定」消耗。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class YinYangGeHunXiao : WanJieRuLinCardModel
{
    public YinYangGeHunXiao() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        SetGhostQiCost(2);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 鬼气费用（卡面用 {GhostQiCost:secondaryResourceIcons()} 渲染成图标+数字）。
        GhostQiCostVarOf(2),
        ModCardVars.Int("DrawPerAttack", 1),
        ModCardVars.Int("ChooseExhaust", 0)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<YinYangGeHunXiaoPower>(choiceContext, 1);
        if (power is not null)
        {
            power.DrawPerAttack = DynamicVars.GetIntOrDefault("DrawPerAttack", 1);
            power.CanChooseExhaust = DynamicVars.GetIntOrDefault("ChooseExhaust", 0) > 0;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DrawPerAttack"].UpgradeValueBy(1);
        DynamicVars["ChooseExhaust"].UpgradeValueBy(1);
    }
}
