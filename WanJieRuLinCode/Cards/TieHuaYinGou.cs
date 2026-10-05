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
/// 线 —— 造成 15 点伤害，获得 8 点格挡，获得 3 点临时力量。
/// 升级 ★ 效果升级：临时力量 3 → 5。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class TieHuaYinGou : WanJieRuLinCardModel
{
    public TieHuaYinGou() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Line;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move),
        new BlockVar(8m, ValueProp.Move),
        ModCardVars.Int("TempStr", 3)
    ];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await LineHit(choiceContext);
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        await GainBlock(choiceContext, DynamicVars.Block.BaseValue);
        await ApplySelf<WanJieTempStrengthPower>(choiceContext, DynamicVars.GetIntOrDefault("TempStr", 3));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["TempStr"].UpgradeValueBy(2);    }
}
