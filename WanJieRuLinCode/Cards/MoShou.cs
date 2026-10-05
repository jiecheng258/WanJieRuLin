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
/// 点 —— 获得 7 点格挡，你每打出过 1 张点牌额外 +3 格挡。
/// 升级 ★ 效果升级：每张 +3 → +4。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MoShou : WanJieRuLinCardModel
{
    public MoShou() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Point;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7m, ValueProp.Move),
        ModCardVars.Int("PerDian", 3)
    ];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PointHit(choiceContext);
        await GainBlock(choiceContext, DynamicVars.Block.BaseValue + MyLuanDian * DynamicVars.GetIntOrDefault("PerDian", 3));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PerDian"].UpgradeValueBy(1);    }
}
