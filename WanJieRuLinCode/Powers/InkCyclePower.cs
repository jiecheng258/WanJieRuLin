using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using WanJieRuLin.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 鬼气循环 —— 能力：每回合结束时把鬼气往均衡区间拉。
/// 鬼气过高（≥8）则失去若干，过低（≤2）则获得若干。
///
/// 「墨匀」流派的自动调平器：它同时压制了「墨浓」的伤害惩罚和「墨淡」的额外抽牌，
/// 换来稳定的中段节奏 —— 正是第三条路线的核心。
/// </summary>
[RegisterPower]
public sealed class InkCyclePower : ModPowerTemplate
{
    /// <summary>未升级时每次调整的量。</summary>
    public const int BaseAmount = 3;

    /// <summary>升级后。</summary>
    public const int UpgradedAmount = 4;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>每次调整量（由卡牌在施加时写入）。</summary>
    public int AmountPerTurn { get; set; } = BaseAmount;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner is not { } owner || !participants.Contains(owner) || owner.Player is not { } player)
        {
            return;
        }

        var qi = GhostQi.Get(player);
        var step = Math.Max(1, AmountPerTurn);

        if (qi >= WanJieRuLinCardModel.InkPhaseDenseMin)
        {
            Flash();
            await GhostQi.Lose(player, step);
        }
        else if (qi <= WanJieRuLinCardModel.InkPhaseThinMax)
        {
            Flash();
            await GhostQi.Gain(player, step);
        }
    }
}
