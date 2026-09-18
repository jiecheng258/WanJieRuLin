using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.FreePlay;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Relics;

/// <summary>
/// 贝雷帽（特殊遗物）—— 战斗开始时获得一张随机技能牌，
/// 该牌可以免费打出一次，随后消耗。
/// </summary>
[RegisterRelic(typeof(WanJieRuLinRelicPool))]
public sealed class BeiLeiMao : WanJieRuLinRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task BeforeCombatStart()
    {
        if (Owner is not { } player)
        {
            return;
        }

        var skillType = WanJieRuLinCardPool.PickRandomSkillType();
        if (skillType is null)
        {
            return;
        }

        // 本角色卡牌都有公开无参构造，直接实例化原型再克隆给玩家。
        if (Activator.CreateInstance(skillType) is not CardModel canonical)
        {
            return;
        }

        Flash();

        var created = canonical.CreateCloneForPlayer(player);

        // 可以免费打出一次，随后消耗。
        CardCmd.ApplyKeyword(created, [CardKeyword.Exhaust]);
        FreePlayBindingRegistry.MarkCardFreeNextPlay(created);

        await CardPileCmd.AddGeneratedCardToCombat(
            created, PileType.Hand, player, CardPilePosition.Random);
    }
}
