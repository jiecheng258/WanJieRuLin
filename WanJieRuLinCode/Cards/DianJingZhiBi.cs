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
/// 点 —— 0 费：抽 2 张牌。若本回合已打出 3 张以上点牌，额外获得 1 点能量。
/// 升级后抽 3 张。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class DianJingZhiBi : WanJieRuLinCardModel
{
    public DianJingZhiBi() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Point;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(2),
        ModCardVars.Int("Energy", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PointHit(choiceContext);
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
        if (MyLuanDian >= 3) { await GainEnergy(DynamicVars.GetIntOrDefault("Energy", 1)); }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);    }
}
