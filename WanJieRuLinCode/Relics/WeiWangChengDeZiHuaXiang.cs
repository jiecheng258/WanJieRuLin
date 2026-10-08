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
/// 未完成的自画像 —— **起始遗物**（v0.7.5 重做）。
///
/// 效果：**每回合你打出的第 1 张牌**，按其归属获得强化，且**只有这一张**：
/// - 该牌是【点】→ 额外获得 [blue]2[/blue] 点能量
/// - 该牌是【线】→ 额外获得 [blue]2[/blue] 点临时力量与 [blue]2[/blue] 点临时敏捷
/// - 该牌是【面】→ 该牌造成的伤害与格挡**翻倍**
///
/// 升级版（<see cref="WeiWangChengDeZiHuaXiangQuan"/>）：三类**各**可触发一次。
///
/// ★ 设计意图：把「本回合的第一手」变成一个明确的决策点 ——
///   玩家要自己判断「这一手用点、线还是面来起手最赚」，
///   而不是无脑铺牌。翻倍给在第 1 张上，收益大但只能用一次，决策感强。
/// </summary>
[RegisterRelic(typeof(WanJieRuLinRelicPool))]
[RegisterCharacterStarterRelic(typeof(WanJieRuLinCharacter))]
public class WeiWangChengDeZiHuaXiang : WanJieRuLinRelic
{
    /// <summary>「点」牌第 1 张额外获得的能量。</summary>
    public const int PointEnergy = 2;

    /// <summary>「线」牌第 1 张额外获得的临时力量/敏捷。</summary>
    public const int LineTempStat = 2;

    // 本回合是否已经用过「第 1 张牌」的强化
    protected bool Used;

    // 「面」翻倍：BeforeCardPlayed 打标记 → 由本遗物的伤害/格挡钩子翻倍
    protected bool FaceDoubling;

    public override RelicRarity Rarity => RelicRarity.Starter;

    /// <summary>升级版覆盖为 true，表示点/线/面三类各自可触发一次。</summary>
    protected virtual bool PerAspect => false;

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
        {
            Used = false;
            FaceDoubling = false;
        }

        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (Owner is null || Used)
        {
            return Task.CompletedTask;
        }

        if (WanJieAspectQuery.Of(cardPlay.Card) == WanJieAspect.Face)
        {
            FaceDoubling = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>「面」牌翻倍：返回 amount，即把伤害翻一倍。</summary>
    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (!FaceDoubling || cardSource is null || dealer != Owner?.Creature)
        {
            return 0m;
        }

        return WanJieAspectQuery.IsFace(cardSource) ? amount : 0m;
    }

    /// <summary>「面」牌格挡翻倍。</summary>
    public override decimal ModifyBlockAdditive(
        Creature? target,
        decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (!FaceDoubling || cardSource is null || target != Owner?.Creature)
        {
            return 0m;
        }

        return WanJieAspectQuery.IsFace(cardSource) ? block : 0m;
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is not { } player)
        {
            return;
        }

        var aspect = WanJieAspectQuery.Of(cardPlay.Card);

        if (Used)
        {
            // 已经用过：只需把「面」的翻倍标记收掉（避免影响后续牌）。
            FaceDoubling = false;
            return;
        }

        switch (aspect)
        {
            case WanJieAspect.Point:
                Used = true;
                Flash();
                await PlayerCmd.GainEnergy(PointEnergy, player);
                break;

            case WanJieAspect.Line:
                Used = true;
                Flash();
                await PowerCmd.Apply<WanJieTempStrengthPower>(
                    choiceContext, player.Creature, LineTempStat, player.Creature, null);
                await PowerCmd.Apply<WanJieTempDexterityPower>(
                    choiceContext, player.Creature, LineTempStat, player.Creature, null);
                break;

            case WanJieAspect.Face:
                Used = true;
                Flash();
                FaceDoubling = false;
                break;

            default:
                // 不属于点线面（打击/防御/能力/先古/事件）→ 不消耗这次机会
                FaceDoubling = false;
                break;
        }
    }
}
