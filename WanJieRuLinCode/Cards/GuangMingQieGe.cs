using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 敌人失去 10 点力量。[消耗] 升级后 ★ 质变：追加抽 1 张牌。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class GuangMingQieGe : WanJieRuLinCardModel
{
    public GuangMingQieGe() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("StrengthLoss", 10),
        ModCardVars.Cards(0)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await ApplyTo<StrengthPower>(choiceContext, cardPlay.Target, -DynamicVars.GetIntOrDefault("StrengthLoss", 10));
        if (DynamicVars.Cards.IntValue > 0)
        {
            await Draw(choiceContext, DynamicVars.Cards.IntValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);    }
}
