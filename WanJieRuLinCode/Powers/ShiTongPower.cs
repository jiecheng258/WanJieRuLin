using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 尸瞳 —— 能力：**本回合**每打出一张牌，获得 1 点鬼气。
///
/// 继承 <see cref="WanJieTurnScopedPower"/>：必须真的在回合结束时撤掉。
/// 否则「每打一张牌 +1 鬼气」是永久生效的 —— 第 3 张牌之后就是净赚，
/// 鬼气在本模组里约等于能量，等于无限资源。
/// </summary>
[RegisterPower]
public sealed class ShiTongPower : WanJieTurnScopedPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner || Owner.Player is not { } player)
        {
            return;
        }

        Flash();
        await GhostQi.Gain(player, 1);
    }
}
