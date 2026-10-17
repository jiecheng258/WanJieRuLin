using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

// ============================================================================
// 作祟张数 / 费用 / 去消耗能力（v0.9）
//
//   - 作祟掌 ZuoSuiZhangPower：作祟影响的张数 +2（升级 +3）
//   - 鬼罗   GuiLuoPower：作祟影响的张数翻倍（升级后翻倍且 +2）
//   - 作祟精 ZuoSuiJingPower：作祟费用减半向下取整（升级后费用 -3 鬼气）
//   - 鬼墨真言 GuiMoZhenYanPower：每回合 +2 鬼气，作祟不再 [消耗]（升级每回合 +3）
//
// 张数/费用/去消耗都由 ZuoSui 牌 OnPlay 时读取这些能力的层数与标志。
// ============================================================================

/// <summary>作祟掌：作祟影响的张数 +2（升级 +3）。</summary>
[RegisterPower]
public sealed class ZuoSuiZhangPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("ExtraCount", 2m),
    ];
}

/// <summary>鬼罗：作祟影响的张数翻倍（升级后翻倍且 +2）。</summary>
[RegisterPower]
public sealed class GuiLuoPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("BonusCount", 2m),
    ];
}

/// <summary>作祟精：作祟费用减半向下取整（升级后费用 -3 鬼气）。</summary>
[RegisterPower]
public sealed class ZuoSuiJingPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>作祟费用减免的鬼气量（由作祟精牌设 3，作祟引牌设为 1/2）。</summary>
    public int CostReduction { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("CostReduction", 3m),
    ];
}

/// <summary>
/// 鬼墨真言：每回合开始 +2 鬼气（升级 +3），作祟不再 [消耗]。
/// </summary>
[RegisterPower]
public sealed class GuiMoZhenYanPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsUpgraded { get; set; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("GhostQiPerTurn", 2m),
    ];

    /// <summary>作祟是否不再 [消耗]（此能力存在即为 true）。</summary>
    public bool ZuoSuiNoExhaust => true;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner?.Player)
        {
            return;
        }

        await GhostQi.Gain(player, IsUpgraded ? 3 : 2);
    }
}
