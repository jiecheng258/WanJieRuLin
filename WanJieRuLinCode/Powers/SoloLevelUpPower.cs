using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 独自升级 —— 能力：下回合开始时随机获得 Amount 点能量或 Amount 点鬼气。
/// </summary>
[RegisterPower]
public sealed class SoloLevelUpPower : ModPowerTemplate
{
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

        var amount = (int)Amount;
        if (amount <= 0)
        {
            return;
        }

        Flash();

        if (Random.Shared.Next(2) == 0)
        {
            await PlayerCmd.GainEnergy(amount, player);
        }
        else
        {
            await GhostQi.Gain(player, amount);
        }

        await PowerCmd.Remove(this);
    }
}
