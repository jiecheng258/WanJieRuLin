using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 我不玩了 —— 能力：之后回合无法获得鬼气；接下来 <see cref="FreeCardsRemaining"/> 张牌可以免费打出。
/// 鬼气获取通过 <see cref="ShouldGainSecondaryResource"/> 直接拒绝。
/// </summary>
[RegisterPower]
public sealed class WoBuWanLePower : ModPowerTemplate, ISecondaryResourceHookListener
{
    /// <summary>剩余可免费打出的牌数（由卡牌设置）。</summary>
    public int FreeCardsRemaining { get; set; }

    /// <summary>本回合已免费打出的张数。</summary>
    private int _freeCardsUsed;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>拒绝一切鬼气获取。</summary>
    public bool ShouldGainSecondaryResource(SecondaryResourceChangeContext context, int amount)
    {
        if (context.Definition.Id != ModResources.GhostQiId || amount <= 0)
        {
            return true;
        }

        return Owner.Player is not { } player || !ReferenceEquals(context.Player, player);
    }

    /// <summary>统计已免费打出的张数，用完后移除本能力。</summary>
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner)
        {
            return;
        }

        _freeCardsUsed++;

        if (_freeCardsUsed >= FreeCardsRemaining)
        {
            await PowerCmd.Remove(this);
        }
    }
}
