using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using STS2RitsuLib.Cards.DynamicVars;

using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 封笔 —— 技能牌 1 能量：获得 9 点格挡，**本回合你无法获得鬼气**。升级后 13 点格挡。
/// ★ v0.3：改为 L 流（蜕鬼）的身份牌 ——「封笔」= 封住鬼气来源，
///   把鬼气**冻在低位**，让「鬼气 ≤2」的条件牌（淡墨/淡描/留白/蜕鬼）稳定生效。
///   原本的「自伤换格挡」与流派无关，且 12 格挡 + 自身虚弱在数值上偏强。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class FengBi : WanJieRuLinCardModel
{
        public FengBi() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(9m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await ApplySelf<StillWaterPower>(choiceContext, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}
