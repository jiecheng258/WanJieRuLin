using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 暂避锋芒 —— 能力：**本回合**获得的格挡翻倍，但无法打出攻击牌。
///
/// 继承 <see cref="WanJieTurnScopedPower"/>：必须真的在回合结束时撤掉。
/// 否则「无法打出攻击牌」这条负面会永久生效，玩家之后再也打不出攻击牌。
/// </summary>
[RegisterPower]
public sealed class BideDefensivelyPower : WanJieTurnScopedPower
{
    /// <summary>本回合内是否禁止打出攻击牌。</summary>
    public bool BlocksAttacks { get; set; } = true;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>本回合获得的格挡翻倍。</summary>
    public override decimal ModifyBlockMultiplicative(
        Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target == Owner && block > 0m)
        {
            return 2m;
        }

        return 1m;
    }

    /// <summary>禁止打出攻击牌。</summary>
    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        if (BlocksAttacks && card.Owner?.Creature == Owner && card.Type == CardType.Attack)
        {
            return false;
        }

        return true;
    }
}
