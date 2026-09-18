using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 土剑 —— 能力：回合开始时获得 1 点覆甲（Plating）。
/// </summary>
[RegisterPower]
public sealed class TuJianPower : ModPowerTemplate
{
    /// <summary>每回合获得的覆甲层数。</summary>
    public const int PlatingPerTurn = 1;

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
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner, PlatingPerTurn * Amount, Owner, (MegaCrit.Sts2.Core.Models.CardModel?)null);
    }
}
