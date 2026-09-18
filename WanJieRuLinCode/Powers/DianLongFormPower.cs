using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 电龙形态 —— 能力：回合开始时获得 10 点活力与 3 点临时力量。
/// Amount 为层数（1 层即一次）。
/// </summary>
[RegisterPower]
public sealed class DianLongFormPower : ModPowerTemplate
{
    /// <summary>回合开始时获得的活力。</summary>
    public const int VigorPerTurn = 10;

    /// <summary>回合开始时获得的临时力量。</summary>
    public const int TemporaryStrengthPerTurn = 3;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<VigorPower>(
            choiceContext, Owner, VigorPerTurn, Owner, (MegaCrit.Sts2.Core.Models.CardModel?)null);
        // 用本模组的临时力量包装能力，来源写成本能力自己。
        await PowerCmd.Apply<DianLongFormTempStrengthPower>(
            choiceContext, Owner, TemporaryStrengthPerTurn, Owner,
            (MegaCrit.Sts2.Core.Models.CardModel?)null);
    }
}
