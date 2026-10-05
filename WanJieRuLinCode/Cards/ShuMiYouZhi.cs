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
/// 点 —— 抽 2 张牌。若本回合已打出 4 张以上点牌（已触发惩罚），再抽 2 张。
/// 升级后基础抽 3 张。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ShuMiYouZhi : WanJieRuLinCardModel
{
    public ShuMiYouZhi() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Point;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PointHit(choiceContext);
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
        if (MyLuanDian >= 4) { await Draw(choiceContext, 2); }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);    }
}
