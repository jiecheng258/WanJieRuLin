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
/// 平行世界 —— 技能牌 0 能量：选择 X 张牌丢入弃牌堆中，抽 X+1 张牌。
/// 升级后获得 1 点能量。
/// 「X」取当前手牌数，实际选择张数由玩家决定。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class PingXingShiJie : WanJieRuLinCardModel
{
    public PingXingShiJie() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("BonusDraw", 1),
        ModCardVars.Energy(0)
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
            return;
        }

        // 让玩家选择要丢弃的牌（最多等于当前手牌数）。
        // 提示文案用原版自带的「弃牌」选择提示 —— 不要用 LocString.KeyPathToLocString，
        // 它在战斗流程里会因为本地化表未就绪而抛 IndexOutOfRangeException，直接卡死回合。
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 0, hand.Count);

        var picked = (await CardSelectCmd.FromHandForDiscard(
            choiceContext, player, prefs, null, this)).ToList();
        if (picked.Count == 0)
        {
            return;
        }

        await CardCmd.Discard(choiceContext, picked);

        var drawCount = picked.Count + DynamicVars.GetIntOrDefault("BonusDraw", 1);
        await CardPileCmd.Draw(choiceContext, drawCount, player);

        var energy = DynamicVars.Energy.BaseValue;
        if (energy > 0)
        {
            await GainEnergy(energy);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}
