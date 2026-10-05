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
/// 点 —— 抽 1 张牌。若本回合已打出 2 张以上点牌，再抽 2 张。
/// 「林」的身份一直是个谜。
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
        ModCardVars.Cards(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PointHit(choiceContext);
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
        if (MyLuanDian >= 2) { await Draw(choiceContext, 2); }
    }

    protected override void OnUpgrade()
    {
    }
}
