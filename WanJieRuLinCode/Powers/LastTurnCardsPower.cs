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
/// 上回合出牌记录 —— 每次打出一张牌时记下来，回合结束时把「上回合」的列表换成「本回合」的。
/// 「悠悠电龙」用它来重放上回合打出的所有牌。
/// </summary>
[RegisterPower]
public sealed class LastTurnCardsPower : ModPowerTemplate
{
    private readonly List<CardModel> _lastTurn = [];
    private readonly List<CardModel> _thisTurn = [];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>上回合打出的所有牌（副本列表，可安全遍历）。</summary>
    public IReadOnlyList<CardModel> LastTurnCards => _lastTurn;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature == Owner)
        {
            _thisTurn.Add(cardPlay.Card);
        }

        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return Task.CompletedTask;
        }

        _lastTurn.Clear();
        _lastTurn.AddRange(_thisTurn);
        _thisTurn.Clear();
        return Task.CompletedTask;
    }
}
