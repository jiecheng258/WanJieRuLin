using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 水剑 —— 能力：回合开始时，弃牌堆中的一张随机攻击牌获得「消耗」与单回合「保留」。
///
/// 实现说明：重放（Replay）在原版里是 <see cref="CardModel.BaseReplayCount"/>，
/// 不是关键字；这里按卡面文案实现为「消耗 + 单回合保留」，保证效果可见且不崩。
/// </summary>
[RegisterPower]
public sealed class ShuiJianPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return Task.CompletedTask;
        }

        var attacks = CardPile.GetCards(player, [PileType.Discard])
            .Where(c => c.Type == CardType.Attack)
            .ToList();

        if (attacks.Count == 0)
        {
            return Task.CompletedTask;
        }

        Flash();

        var target = attacks[Random.Shared.Next(attacks.Count)];

        // 赋予消耗词条（使这张牌被再次打出后消失），并给它单回合保留以便回到手牌。
        CardCmd.ApplyKeyword(target, [CardKeyword.Exhaust]);
        target.GiveSingleTurnRetain();

        return Task.CompletedTask;
    }
}
