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
/// 光明切割 —— 技能牌 1 能量：降低目标 10 点力量，消耗。升级后能量消耗为 0。
/// 对高力量敌人是解药；对低力量敌人则可能反成助力——「光明」的双面性。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class GuangMingQieGe : WanJieRuLinCardModel
{
    private const int StrengthReduction = -10;

    public GuangMingQieGe() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("StrengthLoss", 10)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await ApplyTo<StrengthPower>(choiceContext, cardPlay.Target, StrengthReduction);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
