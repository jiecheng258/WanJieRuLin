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
/// ★ **线** —— 获得 2 点[gold]力道[/gold]。本回合每次获得力道时额外 +1。
/// 升级后额外 +2。★ 和「勾线」这类牌形成滚雪球。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ZongHeng : WanJieRuLinCardModel
{
    public ZongHeng() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Line;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Force", 2),
        ModCardVars.Int("Bonus", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<LiDaoPower>(choiceContext, DynamicVars.GetIntOrDefault("Force", 2));
        var power = await ApplySelfAndGet<ZongHengPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.Amount = DynamicVars.GetIntOrDefault("Bonus", 1);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Bonus"].UpgradeValueBy(1);    }
}
