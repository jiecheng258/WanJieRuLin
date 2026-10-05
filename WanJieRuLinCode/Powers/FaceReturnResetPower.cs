using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 「去年今日此门中」的记录器 —— 面牌回流（每张牌每场一次）的**每场重置开关**。
///
/// 面牌打出时会把自己「已经回流过」这件事记在
/// <see cref="WanJieFaceReturn"/> 的静态表里；那张表在**每场战斗开始时清空**，
/// 本能力就是那个「开战信号」。
///
/// 它不可获取、不进卡池，由起始遗物在战斗开始时挂上。
/// </summary>
[RegisterPower]
public sealed class FaceReturnResetPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override Task BeforeCombatStart()
    {
        WanJieFaceReturn.Reset();
        return Task.CompletedTask;
    }
}
