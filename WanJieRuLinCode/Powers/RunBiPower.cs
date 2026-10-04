using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using WanJieRuLin.Aspects;

namespace WanJieRuLin.Powers;

/// <summary>
/// 润笔 —— ★ **偏激流 · 纯点** 的支撑能力。
///
/// 效果：**每当你打出一张「点」牌，获得 1 点能量**（每回合上限 {Cap} 次）。
///
/// 设计意图：让「只堆点牌」变成一条能赢的路线 —— 点牌本身数值低，
/// 但打出它就能换来能量，于是「点牌 = 燃料」。
/// 有每回合上限，避免变成无限循环（这是本模组反无限的一贯做法）。
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
        ModCardVars.Int("Cap", WanJieV05Tuning.RunBiEnergyCap)
    ];

    /// <summary>每回合可触发的次数上限。</summary>
    public int Cap { get; set; } = WanJieV05Tuning.RunBiEnergyCap;

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
        await PlayerCmd.GainEnergy(1, player);
    }
}
