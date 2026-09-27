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
/// 光明切割 —— 技能牌 1 能量：敌人失去 10 点力量，[消耗]。升级后追加抽 1 张牌。
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
        ModCardVars.Int("StrengthLoss", 10),
        ModCardVars.Cards(0)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await ApplyTo<StrengthPower>(choiceContext, cardPlay.Target, StrengthReduction);

        if (DynamicVars.Cards.IntValue > 0)
        {
            await Draw(choiceContext, DynamicVars.Cards.IntValue);
        }
    }

    protected override void OnUpgrade()
    {
        // ★ 质变：降费 → 加抽牌（原版的「降费」手法这里换成「加抽牌」，收益更可见）。
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
