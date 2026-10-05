using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace WanJieRuLin.Aspects;

// ============================================================================
// 「点 · 线 · 面」—— v0.5 全新框架
//
// 所有攻击牌与技能牌都必须属于三类之一；能力牌不属于任何一类，
// 它们的作用是**延伸三类**（强化某类、降低某类费用、改变某类规则）。
//
//   点 —— 低费、数值偏低、**降低其他牌的费用**
//   线 —— 中等费用、数值持平、**增强其他牌的强度**
//   面 —— 高费、数值偏高、**大收益**
// ============================================================================

/// <summary>卡牌的「笔法」归属。</summary>
public enum WanJieAspect
{
    /// <summary>无归属（能力牌、衍生物、打击/防御等基础牌）。</summary>
    None = 0,

    /// <summary>点 —— 低费、削费。</summary>
    Point = 1,

    /// <summary>线 —— 均衡、增幅。</summary>
    Line = 2,

    /// <summary>面 —— 高费、高收益。</summary>
    Face = 3,
}

/// <summary>
/// 带「点 / 线 / 面」归属的卡牌。由生成的卡牌实现。
/// </summary>
public interface IWanJieAspectCard
{
    /// <summary>本牌的笔法归属。</summary>
    WanJieAspect Aspect { get; }
}

/// <summary>归属判定的小工具。</summary>
public static class WanJieAspectQuery
{
    /// <summary>取一张牌的归属；不属于三类时返回 <see cref="WanJieAspect.None"/>。</summary>
    public static WanJieAspect Of(CardModel? card)
        => card is IWanJieAspectCard a ? a.Aspect : WanJieAspect.None;

    /// <summary>是否为「点」或「线」—— 这两类会被高墨韵削弱。</summary>
    public static bool IsPointOrLine(CardModel? card)
    {
        var a = Of(card);
        return a is WanJieAspect.Point or WanJieAspect.Line;
    }

    /// <summary>是否为「面」。</summary>
    public static bool IsFace(CardModel? card) => Of(card) == WanJieAspect.Face;
}


// ============================================================================
//  三条主机制的数值常量（集中在这里，方便调平衡）
// ============================================================================

/// <summary>v0.5 三机制的可调常量。</summary>
public static class WanJieV05Tuning
{
    /// <summary>墨韵每满这么多层，「点 / 线」牌的效果 −1。</summary>
    public const int MoYunPenaltyStep = 5;

    /// <summary>「点 / 线」牌效果被削的下限（不会削到 0 以下）。</summary>
    public const int MoYunPenaltyFloor = 1;

    /// <summary>
    /// ★ v0.6：墨韵每满一档，「面」牌获得的伤害/格挡加成。
    /// 这是墨韵的**回报侧** —— 没有它，墨韵就是一个只会变重的纯减益。
    /// </summary>
    public const int MoYunFaceBonus = 2;

    /// <summary>
    /// ★ v0.6.2：「笔锋」改为**直接产能量**（每回合最多触发该次数）。
    ///
    /// 原本的「减费」需要钩住原版的费用计算，而那个虚方法是
    /// `private protected` —— 模组无法重写。改走产能量（工程内已验证的路径）。
    /// </summary>
    public const int BiFengTriggerCap = 3;

    /// <summary>偏激流·纯点「润笔」：每回合由点牌获得能量的上限。</summary>
    public const int RunBiEnergyCap = 2;

    /// <summary>偏激流·纯线「力透纸背」：力道每回合衰减量。</summary>
    public const int LiTouDecay = 2;
}
