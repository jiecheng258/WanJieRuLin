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
/// 线 —— 获得 9 点格挡，额外获得 2 点临时敏捷。
/// 升级 ★ 效果升级：临时敏捷 2 → 3。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ChanSi : WanJieRuLinCardModel
{
    public ChanSi() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Line;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(9m, ValueProp.Move),
        ModCardVars.Int("TempDex", 2)
    ];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await LineHit(choiceContext);
        await GainBlock(choiceContext, DynamicVars.Block.BaseValue);
        await ApplySelf<WanJieTempDexterityPower>(choiceContext, DynamicVars.GetIntOrDefault("TempDex", 2));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["TempDex"].UpgradeValueBy(1);    }
}
