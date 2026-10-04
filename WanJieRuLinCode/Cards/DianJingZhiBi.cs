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
/// ★ **点** —— 0 费：获得 3 点[gold]笔锋[/gold]，抽 2 张牌。升级后 4 点笔锋。
/// ★ 一次把费用垫足 + 补手牌，是「面」牌的完美前置。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class DianJingZhiBi : WanJieRuLinCardModel
{
    public DianJingZhiBi() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Point;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Edge", 3),
        ModCardVars.Cards(2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<BiFengPower>(choiceContext, DynamicVars.GetIntOrDefault("Edge", 3));
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Edge"].UpgradeValueBy(1);    }
}
