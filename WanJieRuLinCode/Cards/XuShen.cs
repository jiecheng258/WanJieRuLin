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
/// 虚神 —— 获得 9 格挡，本回合所有伤害 +3。升级：13 格挡，+5 伤害。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class XuShen : WanJieRuLinCardModel
{
    public XuShen() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(9m, ValueProp.Move),
        new DynamicVar("DamageBonus", 3m)
    ];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlock(choiceContext, DynamicVars.Block.BaseValue);

        // 本回合所有伤害 +3（临时力量）
        await ApplySelf<WanJieTempStrengthPower>(choiceContext, IsUpgraded ? 5m : 3m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);          // 9 → 13
        DynamicVars["DamageBonus"].UpgradeValueBy(2m); // 3 → 5
    }
}
