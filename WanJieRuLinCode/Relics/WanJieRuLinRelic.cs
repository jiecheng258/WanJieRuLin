using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Relics;

/// <summary>
/// 万界如林遗物基类：统一把图标指到 res://WanJieRuLin/images/relics/{类名}.png。
/// </summary>
public abstract class WanJieRuLinRelic : ModRelicTemplate
{
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");
}
