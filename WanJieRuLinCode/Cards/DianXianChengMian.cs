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
/// ★ **点 → 面 的转换器** —— 消耗所有[gold]笔锋[/gold]，每 2 点换 1 点[gold]墨韵[/gold]。
/// 升级 ★ 质变：汇率 2:1 → 1:1。
/// ★ 均衡流的关键桥：笔锋这回合用不完，可以存成墨韵。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class DianXianChengMian : WanJieRuLinCardModel
{
    public DianXianChengMian() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Point;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("QiPerMoYun", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var edge = await ClearBiFeng(choiceContext);
        var per = Math.Max(1, DynamicVars.GetIntOrDefault("QiPerMoYun", 2));
        var gain = edge / per;
        if (gain > 0)
        {
            await MoYunPower.Gain(choiceContext, Owner!.Creature, gain);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["QiPerMoYun"].UpgradeValueBy(-1);    }
}
