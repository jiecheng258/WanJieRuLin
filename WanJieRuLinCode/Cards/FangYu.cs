using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 防御 —— 技能牌 1 点能量：获得 5 点格挡。升级后获得 8 点格挡。
/// 4 张作为万界如林的初始卡组。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
[RegisterCharacterStarterCard(typeof(WanJieRuLinCharacter), 4)]
public sealed class FangYu : WanJieRuLinCardModel
{
    public FangYu() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
    }

    public override bool GainsBlock => true;

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
