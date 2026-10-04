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
/// ★ **事件牌** —— 抽 1 张牌；**若你的手牌不多于 1 张**，改为抽 3 张。
/// ★ 「林」的身份一直是个谜 —— 你越接近答案，得到的越多。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class Mi : WanJieRuLinCardModel
{
    public Mi() : base(0, CardType.Skill, CardRarity.Event, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Point;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(1),
        ModCardVars.Int("EmptyDraw", 3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var n = DynamicVars.Cards.IntValue;
        if (HandCountAtMost(1))
        {
            n = DynamicVars.GetIntOrDefault("EmptyDraw", 3);
        }
        await Draw(choiceContext, n);
    }

    protected override void OnUpgrade()
    {
    }
}
