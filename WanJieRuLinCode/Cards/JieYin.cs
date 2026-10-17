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
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 借阴 —— 本回合获得 2 点力量和 2 点敏捷。升级：3 力 3 敏。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class JieYin : WanJieRuLinCardModel
{
    public JieYin() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Strength", 2m),
        new DynamicVar("Dexterity", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var amount = IsUpgraded ? 3m : 2m;

        await ApplySelf<WanJieTempStrengthPower>(choiceContext, amount);
        await ApplySelf<WanJieTempDexterityPower>(choiceContext, amount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Strength"].UpgradeValueBy(1m); // 2 → 3
        DynamicVars["Dexterity"].UpgradeValueBy(1m); // 2 → 3
    }
}
