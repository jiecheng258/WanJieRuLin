using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

// ============================================================================
// 「本回合 +N 临时某属性」的包装能力
//
// 为什么必须用 ModTemporaryAppliedPowerTemplate<来源, 内部能力>：
//
// 塔2 的临时能力是「包装型」的 —— 它自己不做事，只是登记
//   「谁给的（来源）+ 真正生效的是哪个能力（内部能力）」，
// 回合结束时再把内部能力撤回。悬浮提示上会显示这个「来源」。
//
// ⚠️ 血的教训（必读）：
// 曾经继承裸的 ModTemporaryPowerTemplate，并把 OriginModel 实现成
// 「返回自己」（ModelDb.Power<自己>()）。这会造成自引用：
// 渲染这个能力的悬浮提示 → 要取它的来源 → 来源又是它自己 → 无限递归。
// 表现不是抛异常，而是【直接死机】：日志在加载能力图标那一行戛然而止，
// 没有任何 [ERROR]。所以千万别让 OriginModel 指向自己。
//
// 正确做法见下：来源必须是【真正施加这个临时能力的模型】
// （卡牌就写卡牌，能力就写能力）。多个来源就派生多个具体类。
//
// 教程：https://tutorials.sts2modding.com/docs/04-ritsulib/04-05-add-power
//       章节「临时能力」→「多种来源包装」
//
// 通用基类（不带注册属性）：把「图标 / 标题 / 描述」三件事收在一处，
// 让不同来源的临时能力共用一张图和一条文本。
// ============================================================================

/// <summary>
/// 多来源临时能力的共享实现层。
///
/// 具体子类只负责用泛型参数声明「来源」与「内部能力」，
/// 图标与文本通过 <see cref="IconStem"/> / <see cref="LocStem"/> 复用。
/// </summary>
/// <typeparam name="TApplied">来源模型：谁给了这个临时能力。</typeparam>
/// <typeparam name="TInternal">内部能力：真正生效的是哪一个能力。</typeparam>
public abstract class WanJieTempAppliedPower<TApplied, TInternal>
    : ModTemporaryAppliedPowerTemplate<TApplied, TInternal>
    where TApplied : AbstractModel
    where TInternal : PowerModel
{
    /// <summary>图标文件名（不含扩展名）。多个来源共用同一张图。</summary>
    protected abstract string IconStem { get; }

    /// <summary>本地化键前缀（powers 表），实际读取 <c>{LocStem}.title</c> / <c>{LocStem}.description</c>。</summary>
    protected abstract string LocStem { get; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{IconStem}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{IconStem}.png");

    public override LocString Title => new("powers", $"{LocStem}.title");

    /// <summary>
    /// 多个来源共用同一条描述 —— 这是官方教程推荐的做法
    /// （「推荐重载描述，以达到多个 power 共享一条文本的效果」）。
    /// 否则每新增一个来源都要多写一条几乎一样的文案。
    /// </summary>
    public override LocString Description => new("powers", $"{LocStem}.description");
}

/// <summary>
/// 「本回合 +N 临时力量」的包装。派生类只需给出 <c>来源</c> 这一个泛型参数。
///
/// <c>Inherit = true</c> 表示继承它的<b>具体</b>子类会被自动注册，
/// 不用每个都在类上加 <c>[RegisterPower]</c>。
/// </summary>
[RegisterPower(Inherit = true)]
public abstract class WanJieTempStrengthAppliedPower<TApplied>
    : WanJieTempAppliedPower<TApplied, StrengthPower>
    where TApplied : AbstractModel
{
    protected override string IconStem => "WanJieTempStrengthPower";
    protected override string LocStem => "WAN_JIE_RU_LIN_POWER_TEMP_STRENGTH";
    protected override bool IsPositive => true;
}

/// <summary>「本回合 +N 临时敏捷」的包装。用法同 <see cref="WanJieTempStrengthAppliedPower{TApplied}"/>。</summary>
[RegisterPower(Inherit = true)]
public abstract class WanJieTempDexterityAppliedPower<TApplied>
    : WanJieTempAppliedPower<TApplied, DexterityPower>
    where TApplied : AbstractModel
{
    protected override string IconStem => "WanJieTempDexterityPower";
    protected override string LocStem => "WAN_JIE_RU_LIN_POWER_TEMP_DEXTERITY";
    protected override bool IsPositive => true;
}
