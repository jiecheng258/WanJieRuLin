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
/// ★ **线 · 无限流三环** —— 0 费：抽 1 张牌。升级后抽 1 张。
/// ★ 四件套里最朴素的一张，作用是**把环补上**：
///    没有它，回锋和接笔会卡在手牌不够；有了它，手牌才能持续流动。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class XuZhi : WanJieRuLinCardModel
{
    public XuZhi() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Line;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);    }
}
