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
/// 无始无终 —— 无限流四件套的**引擎**（慢启动，但一开就停不下来）。
///
/// 效果：**每回合最多触发 {Cap} 次：你打出「点」牌时，抽 1 张牌。**
///
/// ★ 它是四件套里最贵、最慢的一件（2 费能力），却是把环咬合起来的关键：
///   回锋（挂牌 + 自伤）→ 接笔（看牌产能量）→ 续纸（补牌）→ 无始无终（点牌补手）
///   四张单拎出来都不强，凑齐 + 压薄牌组才会成环。
///   代价是每循环一次掉 1 点生命 —— 无限 = 无限掉血。
///
/// 有每回合上限，避免变成不需要任何代价的无限（本模组一贯的反无限做法）。
/// </summary>
[RegisterPower]
public sealed class WuShiWuZhongPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Cap", 2)
    ];

    /// <summary>每回合可触发的次数上限。</summary>
    public int Cap { get; set; } = 2;

    private int _usedThisTurn;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner is not null && player.Creature == Owner)
        {
            _usedThisTurn = 0;
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

        if (_usedThisTurn >= Math.Max(1, Cap))
        {
            return;
        }

        // 只对「点」牌生效。
        if (WanJieAspectQuery.Of(cardPlay.Card) != WanJieAspect.Point)
        {
            return;
        }

        _usedThisTurn++;
        await CardPileCmd.Draw(choiceContext, 1, player);
    }
}
