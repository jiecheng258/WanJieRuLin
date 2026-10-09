using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 大面·力透 —— ★ 纯面流强化能力（v0.8）。
/// 你的「面」牌回流额外 +1 次（每场每张可回流 2 次）。
/// 供 WanJieFaceReturn.TryClaim 查询。
/// </summary>
[RegisterPower]
public sealed class DaMianLiTouPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>拥有此能力时，每张面牌额外回流 1 次。</summary>
    public static int ExtraReturns(Creature? creature)
        => creature is null ? 0
            : (creature.GetPowerAmount<DaMianLiTouPower>() > 0 ? 1 : 0);
}
