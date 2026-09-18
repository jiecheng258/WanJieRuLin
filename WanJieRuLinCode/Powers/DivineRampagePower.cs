using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 神圣暴走 —— 能力：**本回合**攻击伤害翻倍，但每打出 N 张攻击牌就要额外消耗 1 点鬼气。
/// 鬼气不足时该攻击牌无法打出（由 <see cref="ShouldPlay"/> 拦截）。
///
/// 继承 <see cref="WanJieTurnScopedPower"/>：文案写「本回合」就必须真的在回合结束时撤掉，
/// 否则会变成整场战斗永久翻倍。
/// </summary>
[RegisterPower]
public sealed class DivineRampagePower : WanJieTurnScopedPower
{
    /// <summary>未升级时每多少张攻击牌消耗 1 点鬼气。</summary>
    public const int BaseAttacksPerGhostQi = 1;

    /// <summary>
    /// 能力文案（powers.json 的 description / smartDescription）用到了
    /// <c>{AttacksPerGhostQi}</c>，所以必须在这里声明同名变量。
    /// 只把它写成 C# 属性而不声明 DynamicVar，悬浮提示会渲染失败并在日志里刷
    /// "No source extension could handle the selector named 'AttacksPerGhostQi'"。
    /// </summary>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("AttacksPerGhostQi", BaseAttacksPerGhostQi)
    ];

    /// <summary>每打出多少张攻击牌消耗 1 点鬼气（默认 1，升级后 2）。</summary>
    public int AttacksPerGhostQi { get; set; } = 1;

    /// <summary>本回合已打出的攻击牌数。</summary>
    private int _attacksPlayed;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>攻击牌伤害翻倍。</summary>
    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer == Owner && props.IsPoweredAttack() && amount > 0m)
        {
            return 2m;
        }

        return 1m;
    }

    /// <summary>记录打出的攻击牌，按 N 张扣 1 点鬼气。</summary>
    public override async Task AfterCardPlayed(
        MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner || cardPlay.Card.Type != CardType.Attack)
        {
            return;
        }

        _attacksPlayed++;

        var per = Math.Max(1, AttacksPerGhostQi);
        if (_attacksPlayed % per == 0 && Owner.Player is { } player)
        {
            Flash();
            await GhostQi.Lose(player, 1);
        }
    }
}
