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
/// 面 —— 本回合获得 2 点[gold]临时力量[/gold]，抽 2 张牌。[消耗]
/// 每场战斗首次打出时，将一张此牌的 0 费版本放入弃牌堆。
/// 升级 ★ 效果升级：临时力量改为 3 点。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ChenMo : WanJieRuLinCardModel
{
    public ChenMo() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Face;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Strength", 2),
        ModCardVars.Cards(2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<WanJieTempStrengthPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.Amount = DynamicVars.GetIntOrDefault("Strength", 2);
        }
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
        await ReturnFaceZeroCostCopy(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Strength"].UpgradeValueBy(1);    }
}
