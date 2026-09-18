using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Characters;

public sealed class WanJieRuLinRelicPool : TypeListRelicPoolModel
{
    public override string EnergyColorName => "WanJieRuLin";

    public override string? BigEnergyIconPath => $"{Entry.ResPath}/images/characters/energy_big.png";
    public override string? TextEnergyIconPath => $"{Entry.ResPath}/images/characters/energy_text.png";
}
