using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

// ============================================================================
// 作祟三档强化能力（v0.9）
//
// 这四张能力牌把「作祟」的三个档位（点/线/面）各自强化。
// 它们本身是纯数据能力（存数值），真正的效果注入由 ZuoSuiPower 读取：
//
//   - 点增 DianZengPower：点档（0-1 费）打出时 +1 能量 → +2 能量；升级后额外抽 1 张
//   - 线增 XianZengPower：线档（2 费）额外 +2 格挡；升级后 +4 格挡 +1 力量
//   - 面增 MianZengPower：面档（3+ 费）复制 2 张（而非 1）；升级后复制 3 张
//   - 万界林 WanJieLinPower：三档全强化（点 +2 能量 / 线 +2 力 +2 敏 / 面复制 2 张）；
//                            升级后各再 +1 且作祟不再 [消耗]
//
// ZuoSuiPower.AfterCardPlayed 里通过 GetPowerAmount 读取这些能力的层数，
// 再叠加到对应档位的效果数值上。全部常驻（继承 ModPowerTemplate）。
// ============================================================================

/// <summary>点增：作祟点档（0-1 费）打出时额外 +1 能量（升级后还抽 1 张）。</summary>
[RegisterPower]
public sealed class DianZengPower : ModPowerTemplate, IUpgradablePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>由卡牌 OnPlay 时设置，区分升级前后效果。</summary>
    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("ExtraEnergy", 1m),
        new DynamicVar("ExtraDraw", 1m),
    ];
}

/// <summary>线增：作祟线档（2 费）额外 +2 格挡（升级后 +4 格挡 +1 力量）。</summary>
[RegisterPower]
public sealed class XianZengPower : ModPowerTemplate, IUpgradablePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("ExtraBlock", 2m),
        new DynamicVar("ExtraStrength", 1m),
    ];
}

/// <summary>面增：作祟面档（3+ 费）复制 2 张（升级后复制 3 张）。</summary>
[RegisterPower]
public sealed class MianZengPower : ModPowerTemplate, IUpgradablePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("CopyCount", 2m),
    ];
}

/// <summary>
/// 万界林：作祟三档全强化（点 +2 能量 / 线 +2 力 +2 敏 / 面复制 2 张）。
/// 升级后各再 +1，且作祟不再 [消耗]。
/// </summary>
[RegisterPower]
public sealed class WanJieLinPower : ModPowerTemplate, IUpgradablePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("PointEnergy", 2m),
        new DynamicVar("LineStat", 2m),
        new DynamicVar("FaceCopy", 2m),
    ];

    /// <summary>作祟是否不再 [消耗]（升级后为 true）。</summary>
    public bool ZuoSuiNoExhaust { get; set; }
}
