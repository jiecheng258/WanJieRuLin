using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 火剑 —— 能力：回合开始时获得 1 点临时力量（回合结束消散）。
/// </summary>
[RegisterPower]
public sealed class HuoJianPower : ModPowerTemplate
{
    /// <summary>每回合获得的临时力量。</summary>
    public const int TemporaryStrengthPerTurn = 1;

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
        // 临时力量包装能力的来源写成本能力自己。
        await PowerCmd.Apply<HuoJianTempStrengthPower>(
            choiceContext, Owner, TemporaryStrengthPerTurn * Amount, Owner, (MegaCrit.Sts2.Core.Models.CardModel?)null);
    }
}
