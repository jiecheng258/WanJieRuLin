using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 金剑 —— 能力：回合开始时获得 3 点活力（Vigor）。
/// 飘渺剑诀随机获得的一把剑。
/// </summary>
[RegisterPower]
public sealed class JinJianPower : ModPowerTemplate
{
    /// <summary>每回合获得的活力层数。</summary>
    public const int VigorPerTurn = 3;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner, VigorPerTurn, Owner,
            (MegaCrit.Sts2.Core.Models.CardModel?)null);
    }
}
