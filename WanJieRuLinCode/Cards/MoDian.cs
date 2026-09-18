using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 墨点 —— 技能牌 2 能量：获得 12 点格挡，获得 1 点鬼气。升级后获得 15 点格挡。
/// 「用能量换鬼气」的转化牌，是鬼气经济的重要一环。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MoDian : WanJieRuLinCardModel
{
    private const int GhostQiAmount = 1;

    public MoDian() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(12m, ValueProp.Move),
        GhostQiGainVarOf(GhostQiAmount)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await GainGhostQi(GhostQiAmount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
