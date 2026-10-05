using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 笔锋 —— 「点」牌的核心产出。
///
/// 效果：**本回合内，你打出的下一张牌费用 −{Amount}**（扣到 0 为止），打出后立即清空。
///
/// 设计意图：这是「点 → 面」的桥梁 —— 用便宜的点牌把昂贵的面牌提前抬上场。
///
/// 实现说明：
/// 真正的减费在卡牌侧（<c>WanJieRuLinCardModel</c> 实现
/// <c>ICardEnergyCostContributor.ModifyEnergyCost</c>），读出本方玩家的笔锋层数后扣减。
/// 本能力只负责「持有层数」与「打出后清空」。
/// 继承 <see cref="WanJieTurnScopedPower"/> 保证回合结束自动撤掉。
/// </summary>
[RegisterPower]
public sealed class BiFengPower : WanJieTurnScopedPower
{
    public override PowerType Type => PowerType.Buff;

    /// <summary>可叠加：每层让下一张牌便宜 1 费。</summary>
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    /// <summary>本回合是否已经用掉笔锋（用掉后不再减费）。</summary>
    private bool _consumed;

    /// <summary>取某生物当前的笔锋层数（没有该能力时为 0）。</summary>
    public static int Of(Creature? creature)
    {
        if (creature is null)
        {
            return 0;
        }

        var p = creature.GetPowerAmount<BiFengPower>();
        return p > 0 ? p : 0;
    }
}
