using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using STS2RitsuLib.Cards.DynamicVars;

namespace WanJieRuLin.Cards;

/// <summary>
/// 封笔 —— 技能牌 1 能量：获得 12 点格挡，自身获得 2 层虚弱。升级后自身只获得 1 层虚弱。
/// 以自伤换高格挡的防守牌。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class FengBi : WanJieRuLinCardModel
{
    private const int WeakAmount = 2;

    public FengBi() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(12m, ValueProp.Move),
        ModCardVars.Int("Weak", WeakAmount)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await ApplySelf<WeakPower>(choiceContext, DynamicVars.GetIntOrDefault("Weak", WeakAmount));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Weak"].UpgradeValueBy(-1);
    }
}
