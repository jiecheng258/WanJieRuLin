using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 阴阳割昏晓 —— 能力：
/// 每打出 1 张攻击牌抽 1 张牌；每打出 1 张技能牌随机消耗 1 张手牌。
/// 升级后抽 2 张牌，且改为「指定」消耗（由卡牌以 DrawPerAttack = 2 施加）。
///
/// Amount 语义：
/// - 1 位：每张攻击牌抽 1 张（升级版为 2）
/// - 十位：未使用
/// 这里用两个 DynamicVar 更清晰：DrawPerAttack / ChoiceExhaust。
/// </summary>
[RegisterPower]
public sealed class YinYangGeHunXiaoPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>每张攻击牌抽牌数（默认 1，升级 2）。</summary>
    public int DrawPerAttack { get; set; } = 1;

    /// <summary>是否可指定要消耗的牌（升级后为 true）。</summary>
    public bool CanChooseExhaust { get; set; }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 只响应自己的出牌。
        if (cardPlay.Card.Owner?.Creature != Owner)
        {
            return;
        }

        var pileType = cardPlay.Card.Pile?.Type;
        if (pileType is not (PileType.Hand or PileType.Play))
        {
            return;
        }

        switch (cardPlay.Card.Type)
        {
            case CardType.Attack:
                Flash();
                var draws = DrawPerAttack;
                if (draws > 0 && Owner.Player is { } p)
                {
                    for (var i = 0; i < draws; i++)
                    {
                        await CardPileCmd.Draw(choiceContext, 1, p);
                    }
                }

                break;

            case CardType.Skill:
                await ExhaustOneFromHand(choiceContext);
                break;
        }
    }

    private async Task ExhaustOneFromHand(PlayerChoiceContext choiceContext)
    {
        if (Owner.Player is not { } player)
        {
            return;
        }

        var hand = CardPile.GetCards(player, [PileType.Hand]).ToList();

        if (hand.Count == 0)
        {
            return;
        }

        Flash();

        CardModel? victim;
        if (CanChooseExhaust)
        {
            // 注意：不要用 LocString.KeyPathToLocString —— 它需要游戏的本地化表
            // 在运行期已初始化，在战斗流程里调用会抛 IndexOutOfRangeException，
            // 直接把回合循环打死（表现为「卡死、打不出牌」）。
            // 这里用原版自带的「消耗」选择提示，既安全又已经是本地化好的文案。
            var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1);
            var picked = await CardSelectCmd.FromHand(choiceContext, player, prefs, null, this);
            victim = picked.FirstOrDefault();
        }
        else
        {
            victim = hand[Random.Shared.Next(hand.Count)];
        }

        if (victim is not null)
        {
            await CardCmd.Exhaust(choiceContext, victim, false, false);
        }
    }
}
