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
/// ★ **点** —— 0 费：获得 1 点[gold]笔锋[/gold]，抽 1 张牌。升级后 3 点笔锋。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class DianJing : WanJieRuLinCardModel
{
    public DianJing() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Point;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Edge", 2),
        ModCardVars.Cards(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<BiFengPower>(choiceContext, DynamicVars.GetIntOrDefault("Edge", 2));
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Edge"].UpgradeValueBy(1);    }
}
