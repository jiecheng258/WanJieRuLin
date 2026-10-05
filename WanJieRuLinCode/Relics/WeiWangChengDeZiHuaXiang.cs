using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Aspects;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Relics;

/// <summary>
/// 未完成的自画像 —— **起始遗物**（v0.7 重做）。
///
/// 每回合各一次，三类牌首次打出时分别强化：
/// - **点**：额外获得 [blue]1[/blue] 点能量
/// - **线**：额外获得 [blue]1[/blue] 点临时力量，并抽 [blue]1[/blue] 张牌
/// - **面**：该牌伤害与格挡 [blue]+5[/blue]（当场生效）
///
/// ★ 设计意图：它是一张「教学卡」——
///   逼玩家每回合把三类都碰一次，才能拿满三份奖励；
///   同时又直接演示了三条线各自的定位（点=费用、线=数值与手牌、面=爆发）。
///   强度给得克制（各一次），不会喧宾夺主。
///
/// ★ 实现要点：
///   - 点/线走 <c>AfterCardPlayed</c>（该钩子带 PlayerChoiceContext，可以施法）
///   - 面必须在牌**生效之前**加成 → 用 <c>BeforeCardPlayed</c> 打标记，
///     再由本遗物的 <c>ModifyDamageAdditive</c> / <c>ModifyBlockAdditive</c> 加上去。
///     （<c>BeforeCardPlayed(CardPlay)</c> 没有 context，不能在里面 await 施法）
/// </summary>
[RegisterRelic(typeof(WanJieRuLinRelicPool))]
[RegisterCharacterStarterRelic(typeof(WanJieRuLinCharacter))]
public sealed class WeiWangChengDeZiHuaXiang : WanJieRuLinRelic
{
    /// <summary>「面」牌首次打出时的伤害/格挡加成。</summary>
    public const int FaceBonus = 5;

    // 本回合三类是否已经触发过
    private bool _pointUsed;
    private bool _lineUsed;
    private bool _faceUsed;

    // 「面」加成：BeforeCardPlayed 打标记 → 本遗物的伤害/格挡钩子据此加成
    private bool _faceBonusPending;

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
        {
            return Task.CompletedTask;
        }

        _pointUsed = false;
        _lineUsed = false;
        _faceUsed = false;
        _faceBonusPending = false;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 牌打出**之前**：若是本回合第一张「面」牌，打上待加成标记。
    /// </summary>
    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (Owner is null || _faceUsed)
        {
            return Task.CompletedTask;
        }

        if (WanJieAspectQuery.Of(cardPlay.Card) == WanJieAspect.Face)
        {
            _faceBonusPending = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>「面」牌的伤害加成（只在待加成标记为真时生效）。</summary>
    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (!_faceBonusPending || cardSource is null || cardPlay is null)
        {
            return 0m;
        }

        if (dealer != Owner?.Creature)
        {
            return 0m;
        }

        return WanJieAspectQuery.IsFace(cardSource) ? FaceBonus : 0m;
    }

    /// <summary>「面」牌的格挡加成。</summary>
    public override decimal ModifyBlockAdditive(
        Creature? target,
        decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (!_faceBonusPending || cardSource is null || target != Owner?.Creature)
        {
            return 0m;
        }

        return WanJieAspectQuery.IsFace(cardSource) ? FaceBonus : 0m;
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
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
                await PlayerCmd.GainEnergy(1, player);
                break;

            case WanJieAspect.Line when !_lineUsed:
                _lineUsed = true;
                Flash();
                await PowerCmd.Apply<WanJieTempStrengthPower>(
                    choiceContext, player.Creature, 1, player.Creature, null);
                await CardPileCmd.Draw(choiceContext, 1, player);
                break;

            case WanJieAspect.Face when !_faceUsed:
                _faceUsed = true;
                _faceBonusPending = false;
                Flash();
                break;
        }
    }
}
