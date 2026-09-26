using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 平行世界 —— 技能牌 1 能量：弃掉任意张手牌（X），抽 X+2 张牌。
///
/// 「X」取当前手牌数，实际弃几张由玩家决定。
///
/// ★ 反无限（R1 / R7）—— 旧写法是 **0 费**，且升级后**产能量**：
///   「0 费；弃 X → 抽 X+1；升级后 +1 能量」。
///   0 费 + 抽牌 + 产能量三者齐全，正是净资源为正的典型。
///
///   收紧：
///     1. **0 费 → 1 费**（R1）
///     2. **去掉产能量**（R7：抽牌与产能量不能同卡）
///
///   现在的净效果是「手牌张数不变、内容变好」—— 纯筛选，不产生资源，
///   所以无法自我维持。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class PingXingShiJie : WanJieRuLinCardModel
{
    /// <summary>在「弃多少抽多少」之上额外抽的牌数。</summary>
    private const int BonusDraw = 2;

    public PingXingShiJie() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("BonusDraw", BonusDraw)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is not { } player)
        {
            return;
        }

        var hand = CardPile.GetCards(player, [PileType.Hand]).ToList();
        if (hand.Count == 0)
        {
            // 手上没牌就只结算固定抽牌，不至于空放。
            await CardPileCmd.Draw(choiceContext, DynamicVars.GetIntOrDefault("BonusDraw", BonusDraw), player);
            return;
        }

        // 提示文案用原版自带的「弃牌」选择提示 —— 不要用 LocString.KeyPathToLocString，
        // 它在战斗流程里会因为本地化表未就绪而抛 IndexOutOfRangeException，直接卡死回合。
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 0, hand.Count);

        var picked = (await CardSelectCmd.FromHandForDiscard(
            choiceContext, player, prefs, null, this)).ToList();

        if (picked.Count > 0)
        {
            await CardCmd.Discard(choiceContext, picked);
        }

        var draw = picked.Count + DynamicVars.GetIntOrDefault("BonusDraw", BonusDraw);
        await CardPileCmd.Draw(choiceContext, draw, player);
    }

    protected override void OnUpgrade()
    {
        // ★ 质变：额外抽牌 2 → 3。
        DynamicVars["BonusDraw"].UpgradeValueBy(1);
    }
}
