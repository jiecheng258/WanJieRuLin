using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 傲慢 —— 能力：**本回合**你造成的伤害会等量转化为鬼气。
/// 每次造成伤害后，按实际伤害值获得鬼气。
///
/// 继承 <see cref="WanJieTurnScopedPower"/>：必须真的在回合结束时撤掉。
/// 否则 1:1 的「伤害→鬼气」转换会持续整场战斗，而鬼气在本模组里约等于能量，
/// 等于把每一次伤害都变成了等量能量（一回合打出 60 伤害就有 20 鬼气的上限）。
/// </summary>
[RegisterPower]
public sealed class AoManPower : WanJieTurnScopedPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner || Owner.Player is not { } player)
        {
            return;
        }

        var dealt = (int)result.TotalDamage;
        if (dealt <= 0)
        {
            return;
        }

        Flash();
        await GhostQi.Gain(player, dealt);
    }
}
