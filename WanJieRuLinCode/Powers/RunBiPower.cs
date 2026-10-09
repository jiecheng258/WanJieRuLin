using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using WanJieRuLin.Aspects;

namespace WanJieRuLin.Powers;

/// <summary>
/// 润笔 —— ★ **纯点流**的过牌引擎（v0.8.2 修正，与「笔走龙蛇」分工）。
///
/// 效果：每回合你打出的第 1 张「点」牌，额外抽 {Draw} 张牌。
///
/// 与「笔走龙蛇」的分工：
///   - 笔走龙蛇（能量引擎）：每张点牌 +1 能量（铺量）
///   - 润笔（过牌引擎）：每回合第 1 张点牌额外抽牌（保手牌不断）
///   两者叠加 = 「点牌便宜 → 回能量 → 抽牌 → 再点」的滚雪球循环。
///
/// 触发时机：AfterCardPlayed（牌结算完成后）。
/// 收益规则：仅本回合第 1 张点牌触发（用 _usedThisTurn 标记），之后不再抽。
/// </summary>
[RegisterPower]
public sealed class RunBiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Draw", 1)
    ];

    /// <summary>本回合第 1 张点牌额外抽的牌数。</summary>
    public int Draw { get; set; } = 1;

    private bool _usedThisTurn;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner)
        {
            _usedThisTurn = false;
        }
        await Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is not { } player)
        {
            return;
        }

        if (_usedThisTurn)
        {
            return;
        }

        if (WanJieAspectQuery.Of(cardPlay.Card) != WanJieAspect.Point)
        {
            return;
        }

        _usedThisTurn = true;
        await CardPileCmd.Draw(choiceContext, Math.Max(1, Draw), player);
    }
}
