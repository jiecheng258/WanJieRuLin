using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using WanJieRuLin.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 半砚 —— 能力：每回合结束时，若你的鬼气在均衡区间（3–7），获得格挡。
///
/// 「墨匀」流派的基础件：不惩罚也不奖励极端，只奖励「把鬼气端平」。
/// </summary>
[RegisterPower]
public sealed class HalfInkPower : ModPowerTemplate
{
    /// <summary>未升级时每回合结束获得的格挡。</summary>
    public const int BaseBlock = 4;

    /// <summary>升级后。</summary>
    public const int UpgradedBlock = 6;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>每回合结束获得的格挡（由卡牌在施加时写入）。</summary>
    public int BlockPerTurn { get; set; } = BaseBlock;

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
        if (qi < WanJieRuLinCardModel.InkEvenMin || qi > WanJieRuLinCardModel.InkEvenMax)
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(owner, BlockPerTurn, ValueProp.Move, null);
    }
}
