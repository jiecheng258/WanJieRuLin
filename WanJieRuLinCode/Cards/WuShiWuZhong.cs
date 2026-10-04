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
/// ★ **面 · 无限流引擎** —— 每回合最多触发 2 次：
/// 你打出「点」牌时，抽 1 张牌。升级后上限 3 次。
/// ★ 四件套里最贵、最慢的一件，却是把环咬合的关键。
/// ★ 完整链条：回锋（挂牌+自伤）→ 接笔（产能量）→ 续纸（补手牌）→ 无始无终（点牌回手）。
///    代价是每循环一次掉 1 点生命 —— 无限 = 无限掉血，自带死亡倒计时。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class WuShiWuZhong : WanJieRuLinCardModel
{
    public WuShiWuZhong() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Face;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Cap", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<WuShiWuZhongPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.Cap = DynamicVars.GetIntOrDefault("Cap", 2);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cap"].UpgradeValueBy(1);    }
}
