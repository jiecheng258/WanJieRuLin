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
/// 先古 —— 抽 3 张牌，获得 3 点临时力量。[消耗]
/// 每场战斗首次打出时，将一张此牌的 0 费版本放入弃牌堆。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class WanJieRuLinAncient : WanJieRuLinCardModel
{
    public WanJieRuLinAncient() : base(2, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Face;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(3),
        ModCardVars.Int("TempStr", 3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
        await ApplySelf<WanJieTempStrengthPower>(choiceContext, DynamicVars.GetIntOrDefault("TempStr", 3));
        await ReturnFaceZeroCostCopy(choiceContext);
    }

    protected override void OnUpgrade()
    {
    }
}
