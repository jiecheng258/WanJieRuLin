using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;

namespace WanJieRuLin.Characters;

[RegisterCharacter]
public sealed class WanJieRuLinCharacter
    : ModCharacterTemplate<WanJieRuLinCardPool, WanJieRuLinRelicPool, WanJieRuLinPotionPool>
{
    // 墨黑幽紫：和卡框、能量轮廓保持一致。
    public static readonly Color ThemeColor = new(0.70f, 0.52f, 0.98f);

    private const string SceneRoot = $"{Entry.ResPath}/scenes/characters";
    private const string ImageRoot = $"{Entry.ResPath}/images/characters";
    private const string CharacterScenePath = $"{SceneRoot}/WanJieRuLin_character.tscn";
    private const string EnergyCounterScenePath = $"{SceneRoot}/WanJieRuLin_energy_counter.tscn";
    private const string MerchantScenePath = $"{SceneRoot}/WanJieRuLin_merchant.tscn";
    private const string RestSiteScenePath = $"{SceneRoot}/WanJieRuLin_rest_site.tscn";
    private const string CharacterSelectBgScenePath = $"{SceneRoot}/WanJieRuLin_character_select_bg.tscn";

    public override Color NameColor => ThemeColor;
    public override Color EnergyLabelOutlineColor => new(0.36f, 0.20f, 0.52f);
    public override Color MapDrawingColor => ThemeColor;

    // "林"的身份是一个谜——女性角色。
    public override CharacterGender Gender => CharacterGender.Feminine;

    // 血量 70，金币 99（与设定一致）。
    public override int StartingHp => 70;
    public override int StartingGold => 99;

    public override CharacterAssetProfile AssetProfile => new(
        Scenes: new CharacterSceneAssetSet(
            VisualsPath: CharacterScenePath,
            EnergyCounterPath: EnergyCounterScenePath,
            MerchantAnimPath: MerchantScenePath,
            RestSiteAnimPath: RestSiteScenePath),
        Ui: new CharacterUiAssetSet(
            IconTexturePath: $"{ImageRoot}/WanJieRuLin_character_icon.png",
            IconOutlineTexturePath: $"{ImageRoot}/WanJieRuLin_character_icon_outline.png",
            CharacterSelectBgPath: CharacterSelectBgScenePath,
            CharacterSelectIconPath: $"{ImageRoot}/WanJieRuLin_character_select.png",
            CharacterSelectLockedIconPath: $"{ImageRoot}/WanJieRuLin_character_select_locked.png",
            MapMarkerPath: $"{ImageRoot}/WanJieRuLin_map_marker.png"));

    public override string? PlaceholderCharacterId => "ironclad";

    // "林"不需要时间线小故事。
    public override bool RequiresEpochAndTimeline => false;

    public override float AttackAnimDelay => 0f;
    public override float CastAnimDelay => 0f;

    protected override NCreatureVisuals? TryCreateCreatureVisuals()
    {
        return RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(CharacterScenePath);
    }

    public override List<string> GetArchitectAttackVfx()
    {
        return
        [
            "vfx/vfx_attack_slash",
            "vfx/vfx_attack_blunt",
            "vfx/vfx_heavy_blunt",
            "vfx/vfx_bloody_impact",
            "vfx/vfx_rock_shatter"
        ];
    }
}
