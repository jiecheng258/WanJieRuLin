using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Cards;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Relics;

/// <summary>
/// 一把小剑（特殊遗物）—— 战斗开始时随机获得一把剑的效果。
/// 剑的清单与「飘渺剑诀」共用（见 <see cref="Swords"/>）。
/// </summary>
[RegisterRelic(typeof(WanJieRuLinRelicPool))]
public sealed class YiBaXiaoJian : WanJieRuLinRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task BeforeCombatStart()
    {
        if (Owner is not { } player)
        {
            return;
        }

        Flash();
        var ctx = new HookPlayerChoiceContext(
            this, player, player.NetId, GameActionType.Combat);

        await Swords.GrantRandom(ctx, player.Creature, null);
    }
}
