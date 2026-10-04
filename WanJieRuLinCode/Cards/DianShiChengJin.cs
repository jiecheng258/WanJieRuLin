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
/// ★ **点** —— 获得 5 点[gold]笔锋[/gold]。升级后 6 点。
/// ★ 一张牌直接把后面 3 费的面牌变成 0 费 —— 均衡流的发动机。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class DianShiChengJin : WanJieRuLinCardModel
{
    public DianShiChengJin() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Point;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Edge", 5)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<BiFengPower>(choiceContext, DynamicVars.GetIntOrDefault("Edge", 5));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Edge"].UpgradeValueBy(1);    }
}
