using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.FreePlay;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 鬼域 —— 能力：每回合
/// 第 1 张攻击牌伤害翻倍；
/// 第 1 张技能牌免费打出并抽 2 张牌；
/// 第 3 张牌本回合费用变为 0；
/// 第 4 张牌获得重放。
///
/// 实现要点：
/// - 「免费」必须靠 <see cref="FreePlayBindingRegistry.Register"/> 注册的检测器，
///   在出牌<b>前</b>由框架询问，才能真的生效；出牌后补标是没有用的。
/// - 检测器按「本回合已出牌数」判断，每次出完牌后计数 +1。
/// - 「重放」在原版里不是关键字，而是 <see cref="CardModel.BaseReplayCount"/>，
///   所以直接把它 +1，下一次打出这张牌时会多打一次。
/// </summary>
[RegisterPower]
public sealed class GuiYuPower : ModPowerTemplate
{
    /// <summary>第 1 张技能牌额外抽牌数。</summary>
    public const int FirstSkillDraw = 2;

    /// <summary>免费检测器的稳定 ID（可被替换 / 用于诊断）。</summary>
    private const string FreePlayDetectorId = "WanJieRuLin_GuiYuNthCardFree";

    /// <summary>本回合已打出的牌数。</summary>
    private static readonly Dictionary<Player, int> PlayedThisTurn = [];

    /// <summary>本回合是否已经注册过免费检测器（避免重复注册）。</summary>
    private static bool _detectorRegistered;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>
    /// 注册「第 3 张牌起本回合免费」的检测器。只在第一次实例化时注册一次。
    /// 检测器不看具体是哪张牌，而是看「这张牌打出前，本回合已经打了几张牌」；
    /// 由于检测发生在出牌前，此时计数正好是这张牌的序号（0 基）。
    /// </summary>
    public static void EnsureDetectorRegistered()
    {
        if (_detectorRegistered)
        {
            return;
        }

        FreePlayBindingRegistry.Register(FreePlayDetectorId, play =>
        {
            var owner = play.Card.Owner;
            if (owner is null)
            {
                return false;
            }

            if (!PlayedThisTurn.TryGetValue(owner, out var already))
            {
                return false;
            }

            // 第 3 张牌（0 基 index == 2）起免费；拥有者必须真的挂着鬼域。
            return already >= 2 && HasGuiYu(owner);
        });

        _detectorRegistered = true;
    }

    private static bool HasGuiYu(Player player) =>
        player.Creature?.Powers.Any(p => p is GuiYuPower) == true;

    /// <summary>战斗开始时清掉上一场战斗残留的计数，避免跨战斗串味。</summary>
    public override Task BeforeCombatStart()
    {
        if (Owner.Player is { } player)
        {
            PlayedThisTurn[player] = 0;
        }

        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner)
        {
            PlayedThisTurn[player] = 0;
        }

        return Task.CompletedTask;
    }

    /// <summary>第 1 张攻击牌伤害翻倍。</summary>
    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer != Owner || !props.IsPoweredAttack() || amount <= 0m)
        {
            return 1m;
        }

        if (cardSource?.Type != CardType.Attack || Owner.Player is not { } player)
        {
            return 1m;
        }

        // 出牌前计数为 0 ⇒ 这是本回合第 1 张牌；且它确实是攻击牌。
        // 注意：伤害结算在出牌过程中完成，AfterCardPlayed 尚未推进计数，
        // 所以这里直接判断计数为 0 即可。
        return PlayedThisTurn.TryGetValue(player, out var already) && already == 0
            ? 2m
            : 1m;
    }

    /// <summary>出牌后推进计数，并处理第 1 张技能牌与第 4 张牌的重放。</summary>
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner is not { } player || player.Creature != Owner)
        {
            return;
        }

        var index = PlayedThisTurn.TryGetValue(player, out var n) ? n : 0;
        PlayedThisTurn[player] = index + 1;

        // 第 1 张技能牌：额外抽 2 张牌（免费由检测器在出牌前处理）。
        if (index == 0 && cardPlay.Card.Type == CardType.Skill)
        {
            Flash();
            await CardPileCmd.Draw(choiceContext, FirstSkillDraw, player);
        }

        // 第 4 张牌（0 基 index == 3）：获得重放（下一次打出时再打一次）。
        if (index == 3)
        {
            Flash();
            cardPlay.Card.BaseReplayCount += 1;
        }
    }
}
