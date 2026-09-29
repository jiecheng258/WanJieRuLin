using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

// ============================================================================
// v0.4 新增机制（用户要求「机制可以再设计的大胆有趣一点」）
//
//   A 连笔 LianBiPower    —— 本回合每打出一张牌，本回合伤害递增
//   C 伏笔 FuBiPower      —— 打出后不立即生效，数回合后引爆
//   D 相位天气 TianQiPower —— 每回合随机降下一种「墨相」
//   （B 血墨 不需要能力类：卡牌自己受 `CreatureCmd.Damage` 即可）
//
// ⚠️ 全部按工程内**已验证过的 API** 写，不引入新签名：
//      ModifyDamageAdditive / AfterCardPlayed / AfterPlayerTurnStart /
//      CreatureCmd.GainBlock / PlayerCmd.GainEnergy / GhostQi.Gain
// ============================================================================


/// <summary>
/// A · 连笔 —— 本回合内，你**每打出一张牌**，本回合的伤害就 +<see cref="BonusPerStep"/>，
/// 最多叠到 <see cref="MaxSteps"/> 层。
///
/// 设计意图：逼玩家「一口气把牌打完」，与「留牌过回合」形成取舍。
/// 只作用于**本回合**（继承 WanJieTurnScopedPower 会自动撤掉）——
/// 这是全工程踩过最多的坑，见 WanJieTurnScopedPower 的注释。
/// </summary>
[RegisterPower]
public sealed class LianBiPower : WanJieTurnScopedPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("BonusPerStep", 2),
        ModCardVars.Int("MaxSteps", 4)
    ];

    /// <summary>每打出一张牌，本回合伤害 +多少。</summary>
    public int BonusPerStep { get; set; } = 2;

    /// <summary>最多叠加层数。</summary>
    public int MaxSteps { get; set; } = 4;

    /// <summary>本回合已打出的牌数（可能超过上限，实际加伤时取 min）。</summary>
    public int PlayedThisTurn { get; private set; }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (Owner is null)
        {
            return;
        }

        PlayedThisTurn++;
        await Task.CompletedTask;
    }

    /// <summary>本回合实际生效的加伤值。</summary>
    public decimal CurrentBonus =>
        (decimal)Math.Min(PlayedThisTurn, MaxSteps) * BonusPerStep;

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        // 只加自己造成的伤害。
        if (dealer != Owner || cardSource is null)
        {
            return 0m;
        }

        return CurrentBonus;
    }
}


/// <summary>
/// C · 伏笔 —— 打出后**不立即生效**，<see cref="TurnsLeft"/> 个回合后引爆，
/// 对所有敌人造成 <see cref="Damage"/> 点伤害。
///
/// 设计意图：把「现在」变成「以后」，让玩家为远期布局买单；
/// 与需要即时反应的防御/爆发形成节奏差。
///
/// 实现要点：到期后调 <see cref="PowerCmd.Remove"/> 撤掉自己，
/// 否则每次回合开始都会再炸一次（曾经的「本回合能力不自动消失」坑）。
/// </summary>
[RegisterPower]
public sealed class FuBiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Damage", 14),
        ModCardVars.Int("Turns", 2)
    ];

    /// <summary>引爆伤害。</summary>
    public int Damage { get; set; } = 14;

    /// <summary>剩余回合数（由卡牌设置）。</summary>
    public int TurnsLeft { get; set; } = 2;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (Owner is null || player.Creature != Owner)
        {
            return;
        }

        TurnsLeft--;
        if (TurnsLeft > 0)
        {
            return;
        }

        var combatState = Owner.CombatState;
        if (combatState is not null)
        {
            foreach (var enemy in combatState.HittableEnemies.ToList())
            {
                // 签名照抄 GuangMingYuYanPower：
                //   Damage(ctx, 目标, 数值, props, 来源生物, 卡牌, ?)
                await CreatureCmd.Damage(
                    choiceContext, enemy, Damage, ValueProp.Move, Owner, null, null);
            }
        }

        // ★ 到期必须自己撤掉，否则每回合重复引爆。
        await PowerCmd.Remove(this);
    }
}


/// <summary>
/// D · 相位天气 —— 每回合开始时随机降下一种「墨相」：
/// 获得格挡 / 获得能量 / 获得鬼气 / 弃牌堆回手。
///
/// 设计意图：给 M 流（匀墨）一个**不确定但高收益**的引擎，
/// 让玩家学会「围绕随机性做即时决策」，而不是背板子。
///
/// ⚠️ 随机源用 <c>Random.Shared</c>（工程规范：不要 new Random()）。
/// </summary>
[RegisterPower]
public sealed class TianQiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("PhaseAmount", 5)
    ];

    /// <summary>每次降下的强度（格挡/鬼气点数）。</summary>
    public int PhaseAmount { get; set; } = 5;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (Owner is null || player.Creature != Owner)
        {
            return;
        }

        switch (Random.Shared.Next(3))
        {
            case 0:
                // 墨相·厚 —— 获得格挡
                await CreatureCmd.GainBlock(Owner, PhaseAmount, ValueProp.Move, null);
                break;

            case 1:
                // 墨相·润 —— 获得鬼气
                await GhostQi.Gain(player, PhaseAmount);
                break;

            default:
                // 墨相·活 —— 获得能量
                await PlayerCmd.GainEnergy(1, player);
                break;
        }
    }
}
