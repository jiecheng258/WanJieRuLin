using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 临时敏捷（本回合）—— 由「线」牌的千钧一线提供，回合结束消失。
/// </summary>
[RegisterPower]
public sealed class WanJieTempDexterityPower : WanJieTempStatPower
{
    protected override DynamicVar StatVar => new DynamicVar("Dexterity", 1m);

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");
}
