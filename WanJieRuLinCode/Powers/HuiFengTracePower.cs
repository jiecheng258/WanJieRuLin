using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 回锋标记 —— 无限流四件套的**隐藏连接件**（本身不进卡池）。
///
/// 作用：一个「本回合打出过回锋」的标记。由「回锋」打出时自动挂上，
/// 回合结束自动消失（继承 <see cref="WanJieTurnScopedPower"/>）。
/// 「接笔」通过检查这个标记决定能否额外产出能量。
///
/// ★ 设计意图：无限流**不该由单张卡实现**。四件套每一张单拎出来都很弱，
/// 只有凑齐并把牌组压得足够薄才会咬合成环；
/// 代价是每循环一次掉 1 点生命 —— 无限 = 无限掉血，自带死亡倒计时。
///
/// 实现说明：用「标记能力 + HasPower」而不是自定义标志位，
/// 只依赖已验证的 PowerCmd.Apply / HasPower&lt;T&gt;(creature, n)。
/// </summary>
[RegisterPower]
public sealed class HuiFengTracePower : WanJieTurnScopedPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    /// <summary>本方本回合是否已打出过「回锋」。</summary>
    public static bool WasPlayedThisTurn(Creature? creature)
        => creature is not null && creature.HasPower<HuiFengTracePower>(1);
}
