using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using WanJieRuLin.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 墨韵天成 —— 能力：每回合结束时，若鬼气在均衡区间（3–7），抽牌并获得能量。
///
/// 「墨匀」流派的终点。一回合 2 张牌 + 2 点能量，价值约等于半张「回响形态」，
/// 但要一直把鬼气端在 3–7 才拿得到 —— 这是这条路线最难的约束，也是它的回报。
/// </summary>
[RegisterPower]
public sealed class InkRhythmPower : ModPowerTemplate
{
    /// <summary>每回合结束抽的牌数（由卡牌在施加时写入）。</summary>
    public int DrawPerTurn { get; set; } = 2;

    /// <summary>每回合结束获得的能量（由卡牌在施加时写入）。</summary>
    public int EnergyPerTurn { get; set; } = 2;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

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
        await PlayerCmd.GainEnergy(EnergyPerTurn, player);
        await CardPileCmd.Draw(choiceContext, DrawPerTurn, player);
    }
}
