using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 鬼影森森 —— 能力：每造成 <see cref="DamageThreshold"/> 点伤害，获得 1 点鬼气。
/// 升级后阈值降到 <see cref="ReducedDamageThreshold"/>。
///
/// 实现：Amount 记录「当前累计伤害」，阈值存于 DynamicVars["Threshold"]。
/// </summary>
[RegisterPower]
public sealed class GuiYingSenSenPower : ModPowerTemplate
{
    /// <summary>未升级阈值。</summary>
    public const int DamageThreshold = 20;

    /// <summary>升级后阈值。</summary>
    public const int ReducedDamageThreshold = 15;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>
    /// 能力文案（powers.json 的 description / smartDescription）里用到了
    /// <c>{Threshold}</c>，所以必须在这里声明同名变量。
    ///
    /// 注意：能力的 <c>CanonicalVars</c> 在 <c>PowerModel</c> 上是
    /// protected virtual（基类返回 Array.Empty），重写即可，不影响基类行为。
    /// 只把阈值放成 C# 属性而不声明 DynamicVar，悬浮提示会渲染失败并在日志里刷
    /// "No source extension could handle the selector named 'Threshold'"。
    /// </summary>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Threshold", DamageThreshold)
    ];

    /// <summary>累计阈值（默认 20，升级版由卡牌设为 15）。</summary>
    public int Threshold { get; set; } = DamageThreshold;

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer != Owner || result.TotalDamage <= 0)
        {
            return;
        }

        var threshold = Threshold;
        if (threshold <= 0)
        {
            return;
        }

        var accumulated = Amount + (int)result.TotalDamage;
        var gained = accumulated / threshold;

        // 记录余数（无论是否触发都要更新累计值）。
        SetAmount(accumulated % threshold, true);

        if (gained <= 0 || Owner.Player is not { } player)
        {
            return;
        }

        Flash();
        for (var i = 0; i < gained; i++)
        {
            await GhostQi.Gain(player, 1);
        }
    }
}
