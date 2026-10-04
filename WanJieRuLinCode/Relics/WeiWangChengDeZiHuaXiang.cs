using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Aspects;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Relics;

/// <summary>
/// 未完成的自画像 —— **起始遗物**。
///
/// 效果：
/// - 每回合开始时获得 1 点[gold]笔锋[/gold]。
/// - 每当你打出一张「面」牌，获得 1 点[gold]墨韵[/gold]。
///
/// ★ 设计意图：用**最小的数字同时点亮两条线** ——
///   「笔锋」让玩家立刻体会到「点 → 让下一张牌更便宜」，
///   「墨韵」让玩家在打出面牌时看到累积数字往上走。
///   它不给任何数值强度（1 点笔锋 ≈ 每回合省 1 费，属于起始遗物的正常档位），
///   只负责把玩家推上主循环。
///
/// ★ 为什么第一回合的笔锋放在 AfterPlayerTurnStart 而不是 BeforeCombatStart：
///   BeforeCombatStart 钩子**没有 PlayerChoiceContext**，而 PowerCmd.Apply 必须要一个。
///   所以用 _firstTurn 标记，在第一个回合的开头补上，之后每回合正常给。
/// </summary>
[RegisterRelic(typeof(WanJieRuLinRelicPool))]
[RegisterCharacterStarterRelic(typeof(WanJieRuLinCharacter))]
public sealed class WeiWangChengDeZiHuaXiang : WanJieRuLinRelic
{
    /// <summary>每回合开始时获得的笔锋。</summary>
    public const int EdgePerTurn = 1;

    /// <summary>每打出一张「面」牌获得的墨韵。</summary>
    public const int InkPerFaceCard = 1;

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<BiFengPower>(
            choiceContext, player.Creature, EdgePerTurn, player.Creature, null);
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is not { } player)
        {
            return;
        }

        // 只对「面」牌生效。
        if (WanJieAspectQuery.Of(cardPlay.Card) != WanJieAspect.Face)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<MoYunPower>(
            choiceContext, player.Creature, InkPerFaceCard, player.Creature, null);
    }
}
