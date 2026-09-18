using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 墨染江山 —— 先古之民升级卡，替换「绘」。
/// 0 能量 X 鬼气：本回合内提升 X+3 点力量、X+3 点敏捷，保留。
/// 升级后获得固有。
/// 这是一张先古（Ancient）稀有度卡，进入原版先古之民卡池。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MoRanJiangShan : WanJieRuLinCardModel
{
    private const int Bonus = 3;

    public MoRanJiangShan() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
        SetGhostQiCostX();
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Bonus", Bonus)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var x = GhostQiXValue(cardPlay);
        var amount = x + DynamicVars.GetIntOrDefault("Bonus", Bonus);

        if (amount <= 0)
        {
            return;
        }

        // 临时力量 / 敏捷走本模组的包装能力（回合结束自动撤销）。
        // 注意：来源必须写成本卡自己（泛型参数），否则临时能力的「来源显示」
        // 会自引用并导致死机，见 WanJieTempAppliedPower 的说明。
        await ApplySelf<MoRanJiangShanTempStrengthPower>(choiceContext, amount);
        await ApplySelf<MoRanJiangShanTempDexterityPower>(choiceContext, amount);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
