namespace WanJieRuLin.Powers;

/// <summary>
/// 标记「能力本身带升级差异」的接口。卡牌 OnPlay 时判断升级后，
/// 通过 <see cref="IsUpgraded"/> 把升级状态传给能力，能力据此区分效果数值。
/// </summary>
public interface IUpgradablePower
{
    /// <summary>该能力是否处于「卡牌已升级」状态。</summary>
    bool IsUpgraded { get; set; }
}
