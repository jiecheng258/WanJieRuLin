using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 消火 —— 技能牌 1 点鬼气：获得 10 点格挡，给予自身 2 层虚弱。升级后给予自身 1 层虚弱。
/// 与封笔同源，但改为消耗鬼气而非能量。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class XiaoHuo : WanJieRuLinCardModel
{
    public XiaoHuo() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        SetGhostQiCost(1);
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 鬼气费用（卡面用 {GhostQiCost:secondaryResourceIcons()} 渲染成图标+数字）。
        GhostQiCostVarOf(1),
        new BlockVar(10m, ValueProp.Move),
        ModCardVars.Int("Weak", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await ApplySelf<WeakPower>(choiceContext, DynamicVars.GetIntOrDefault("Weak", 2));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Weak"].UpgradeValueBy(-1);
    }
}
