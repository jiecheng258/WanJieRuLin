using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using WanJieRuLin.Cards;

namespace WanJieRuLin.Powers;

/// <summary>
/// 剑雨 —— 「剑！」：接下来 Amount 个回合，每回合开始随机获得 SwordsPerTurn 把剑的效果。
/// 与「飘渺剑诀」共用同一套剑（见 <see cref="Swords"/>）。
/// </summary>
[RegisterPower]
public sealed class SwordStormPower : ModPowerTemplate
{
    /// <summary>每回合获得的剑数（由卡牌设置）。</summary>
    public int SwordsPerTurn { get; set; } = 5;

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

        for (var i = 0; i < SwordsPerTurn; i++)
        {
            await Swords.GrantRandom(choiceContext, player.Creature, null);
        }
    }
}
