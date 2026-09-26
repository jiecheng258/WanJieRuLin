using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using WanJieRuLin.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 薄雾 —— 能力：回合开始时，若你的鬼气处于「墨淡」（≤2），获得能量。
///
/// 「蜕鬼」流派的引擎：清空鬼气本身就是收益，配合墨之相的多抽 1 张，
/// 低鬼气流每回合能比高鬼气流多滚一圈。
/// </summary>
[RegisterPower]
public sealed class MistPower : ModPowerTemplate
{
    /// <summary>未升级时每回合获得的能量。</summary>
    public const int BaseEnergy = 1;

    /// <summary>升级后。</summary>
    public const int UpgradedEnergy = 2;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>墨淡时每回合获得的能量（由卡牌在施加时写入）。</summary>
    public int EnergyPerTurn { get; set; } = BaseEnergy;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner is not { } owner || player.Creature != owner)
        {
            return;
        }

        if (GhostQi.Get(player) > WanJieRuLinCardModel.InkPhaseThinMax)
        {
            return;
        }

        Flash();
        await PlayerCmd.GainEnergy(EnergyPerTurn, player);
    }
}
