using WanJieRuLin.Cards;

namespace WanJieRuLin.Powers;

// ============================================================================
// 各来源的临时力量 / 临时敏捷。
//
// 每个「来源 × 内部能力」组合需要一个具体类 —— 因为来源是写在泛型参数上的
// （塔2 的临时能力悬浮提示要显示「是谁给的」）。
//
// 这些类都通过基类上的 [RegisterPower(Inherit = true)] 自动注册，
// 不需要自己再写 [RegisterPower]。
//
// 图标与文本由 WanJieTempAppliedPower 统一给出（IconStem / LocStem），
// 所以新增来源时**不需要**加图片、也不需要加本地化条目。
// ============================================================================

// ---------------------------------------------------------------- 绘

/// <summary>「绘」给出的临时力量。</summary>
public sealed class HuiTempStrengthPower : WanJieTempStrengthAppliedPower<Hui>;

/// <summary>「绘」给出的临时敏捷。</summary>
public sealed class HuiTempDexterityPower : WanJieTempDexterityAppliedPower<Hui>;

// ---------------------------------------------------------------- 墨染江山

/// <summary>「墨染江山」给出的临时力量。</summary>
public sealed class MoRanJiangShanTempStrengthPower : WanJieTempStrengthAppliedPower<MoRanJiangShan>;

/// <summary>「墨染江山」给出的临时敏捷。</summary>
public sealed class MoRanJiangShanTempDexterityPower : WanJieTempDexterityAppliedPower<MoRanJiangShan>;

// ---------------------------------------------------------------- 黑暗壁障

/// <summary>「黑暗壁障」给出的临时敏捷。</summary>
public sealed class HeiAnBiZhangTempDexterityPower : WanJieTempDexterityAppliedPower<HeiAnBiZhang>;

// ---------------------------------------------------------------- 能力来源

/// <summary>「电龙形态」给出的临时力量。</summary>
public sealed class DianLongFormTempStrengthPower : WanJieTempStrengthAppliedPower<DianLongFormPower>;

/// <summary>「火剑」给出的临时力量。</summary>
public sealed class HuoJianTempStrengthPower : WanJieTempStrengthAppliedPower<HuoJianPower>;
