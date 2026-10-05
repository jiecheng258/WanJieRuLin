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
/// 线 —— 抽 3 张牌，获得 2 点临时力量。
/// 升级 ★ 效果升级：临时力量 2 → 3。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class XingYunLiuShui : WanJieRuLinCardModel
{
    public XingYunLiuShui() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Line;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(3),
        ModCardVars.Int("TempStr", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await LineHit(choiceContext);
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
        await ApplySelf<WanJieTempStrengthPower>(choiceContext, DynamicVars.GetIntOrDefault("TempStr", 2));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["TempStr"].UpgradeValueBy(1);    }
}
