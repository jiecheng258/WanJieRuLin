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
/// ★ **线** —— 获得 2 点[gold]力道[/gold]，抽 2 张牌。升级后 4 点力道。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class YunJinChengFeng : WanJieRuLinCardModel
{
    public YunJinChengFeng() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Line;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Force", 3),
        ModCardVars.Cards(2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<LiDaoPower>(choiceContext, DynamicVars.GetIntOrDefault("Force", 3));
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Force"].UpgradeValueBy(1);    }
}
