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
/// ★ **点 · 无限流二环** —— 0 费：抽 1 张牌。
/// **若你本回合打出过「回锋」**，额外获得 1 点能量。
/// 升级后抽 1 张。
/// ★ 单独用就是一张「0 费抽 1」，很普通。价值在于**给回锋回血**：
///    回锋自伤 → 接笔亮灯产能量 → 能量再喂给下一个循环。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class JieBi : WanJieRuLinCardModel
{
    public JieBi() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Point;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var lit = Owner is { } p && HuiFengTracePower.WasPlayedThisTurn(p.Creature);
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
        if (lit)
        {
            await GainEnergy(1);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);    }
}
