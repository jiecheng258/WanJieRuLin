using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Aspects;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// ★ **事件牌** —— 获得 3 点[gold]力道[/gold]。
/// ★ 她的身世没人说得清，但她说的话总能让人多出一分力气。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class TaDeLaiChu : WanJieRuLinCardModel
{
    public TaDeLaiChu() : base(1, CardType.Skill, CardRarity.Event, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Line;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Force", 3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<LiDaoPower>(choiceContext, DynamicVars.GetIntOrDefault("Force", 3));
    }

    protected override void OnUpgrade()
    {
    }
}
