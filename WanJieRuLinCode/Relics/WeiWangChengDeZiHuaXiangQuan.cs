using MegaCrit.Sts2.Core.Entities.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Aspects;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Relics;

/// <summary>
/// 未完成的自画像 · 全 —— **升级版遗物**（先古之民强化后获得）。
///
/// 效果：每回合【点】【线】【面】**各**第 1 张获得强化（其余同类牌不享受）：
/// - 第 1 张【点】→ 额外 +2 点能量
/// - 第 1 张【线】→ 额外 +2 点临时力量与 +2 点临时敏捷
/// - 第 1 张【面】→ 该牌伤害与格挡翻倍
///
/// ★ 与基础版的差别：基础版每回合一共只强化 1 张牌；
///   升级版强化到 3 张（每类一张），是本游戏最强的一档起手节奏。
///
/// 获得方式：先古之民的强化选项（或作为稀有遗物掉落）。
/// </summary>
[RegisterRelic(typeof(WanJieRuLinRelicPool))]
public sealed class WeiWangChengDeZiHuaXiangQuan : WanJieRuLinRelic
{
    public const int PointEnergy = 2;
    public const int LineTempStat = 2;

    private bool _pointUsed;
    private bool _lineUsed;
    private bool _faceUsed;
    private bool _faceDoubling;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override Task AfterPlayerTurnStart(
        MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (player == Owner)
        {
            _pointUsed = false;
            _lineUsed = false;
            _faceUsed = false;
            _faceDoubling = false;
        }

        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(MegaCrit.Sts2.Core.Entities.Cards.CardPlay cardPlay)
    {
        if (Owner is null || _faceUsed)
        {
            return Task.CompletedTask;
        }

        if (WanJieAspectQuery.Of(cardPlay.Card) == WanJieAspect.Face)
        {
            _faceDoubling = true;
        }

        return Task.CompletedTask;
    }

    public override decimal ModifyDamageAdditive(
        MegaCrit.Sts2.Core.Entities.Creatures.Creature? target,
        decimal amount,
        MegaCrit.Sts2.Core.ValueProps.ValueProp props,
        MegaCrit.Sts2.Core.Entities.Creatures.Creature? dealer,
        MegaCrit.Sts2.Core.Models.CardModel? cardSource,
        MegaCrit.Sts2.Core.Entities.Cards.CardPlay? cardPlay)
    {
        if (!_faceDoubling || cardSource is null || dealer != Owner?.Creature)
        {
            return 0m;
        }

        return WanJieAspectQuery.IsFace(cardSource) ? amount : 0m;
    }

    public override decimal ModifyBlockAdditive(
        MegaCrit.Sts2.Core.Entities.Creatures.Creature? target,
        decimal block,
        MegaCrit.Sts2.Core.ValueProps.ValueProp props,
        MegaCrit.Sts2.Core.Models.CardModel? cardSource,
        MegaCrit.Sts2.Core.Entities.Cards.CardPlay? cardPlay)
    {
        if (!_faceDoubling || cardSource is null || target != Owner?.Creature)
        {
            return 0m;
        }

        return WanJieAspectQuery.IsFace(cardSource) ? block : 0m;
    }

    public override async Task AfterCardPlayed(
        MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Entities.Cards.CardPlay cardPlay)
    {
        if (Owner is not { } player)
        {
            return;
        }

        switch (WanJieAspectQuery.Of(cardPlay.Card))
        {
            case WanJieAspect.Point when !_pointUsed:
                _pointUsed = true;
                Flash();
                await MegaCrit.Sts2.Core.Commands.PlayerCmd.GainEnergy(PointEnergy, player);
                break;

            case WanJieAspect.Line when !_lineUsed:
                _lineUsed = true;
                Flash();
                await MegaCrit.Sts2.Core.Commands.PowerCmd.Apply<WanJieRuLin.Powers.WanJieTempStrengthPower>(
                    choiceContext, player.Creature, LineTempStat, player.Creature, null);
                await MegaCrit.Sts2.Core.Commands.PowerCmd.Apply<WanJieRuLin.Powers.WanJieTempDexterityPower>(
                    choiceContext, player.Creature, LineTempStat, player.Creature, null);
                break;

            case WanJieAspect.Face when !_faceUsed:
                _faceUsed = true;
                _faceDoubling = false;
                Flash();
                break;
        }
    }
}
