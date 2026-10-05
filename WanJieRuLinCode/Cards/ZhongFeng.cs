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
/// 线 —— 获得 4 点临时力量与 4 点临时敏捷。
/// 升级 ★ 效果升级：各 6 点。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ZhongFeng : WanJieRuLinCardModel
{
    public ZhongFeng() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Line;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("TempStr", 4),
        ModCardVars.Int("TempDex", 4)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await LineHit(choiceContext);
        await ApplySelf<WanJieTempStrengthPower>(choiceContext, DynamicVars.GetIntOrDefault("TempStr", 4));
        await ApplySelf<WanJieTempDexterityPower>(choiceContext, DynamicVars.GetIntOrDefault("TempDex", 4));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["TempStr"].UpgradeValueBy(2);        DynamicVars["TempDex"].UpgradeValueBy(2);    }
}
