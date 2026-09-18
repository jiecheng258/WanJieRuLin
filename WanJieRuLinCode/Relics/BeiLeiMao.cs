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

        // 本角色卡牌都有公开无参构造，直接实例化原型。
        // ★ 造牌姿势（IL 反编译确认）：
        //
        // `Activator.CreateInstance` 造出来的是**规范（不可变）**实例，
        // 必须先 `ToMutable()`，否则后面任何写入（加关键字等）都会抛
        // `CanonicalModelException`。
        //
        // 但**不要**在这里手工 `CreateCloneForPlayer(player)`：
        // ① 它的 IL 只有 `CreateClone()` + `_owner = player`，从**不把牌放进堆**；
        //    而 `CreateClone()` 一开头就读 `get_Pile()`，牌没有堆时必然
        //    `NullReferenceException`（这就是先古选项卡死的同一个坑）。
        // ② `CardPileCmd.AddGeneratedCardToCombat` 内部**不做克隆**，
        //    并且会主动拒绝「已经有堆」的牌，抛出：
        //       "You are not allowed to generate cards that already have a pile"
        //    它的实现是：要求牌**已有 Owner** → `CardPileCmd.Add(card, 该玩家堆, ...)`。
        //    所以它要的就是一张「已 ToMutable、已设 Owner、还没进堆」的牌。
        //
        // 于是正确顺序是：ToMutable() → 设 Owner → 交给 AddGeneratedCardToCombat。
        if (Activator.CreateInstance(skillType) is not CardModel canonical)
        {
            return;
        }

        Flash();

        var created = canonical.ToMutable();
        created.Owner = player;

        // 可以免费打出一次，随后消耗。
        // 注意：这些标记必须作用在**最终入堆的那张牌**上，
        // 而 AddGeneratedCardToCombat 不会克隆，所以这里改的就是它本身。
        CardCmd.ApplyKeyword(created, [CardKeyword.Exhaust]);
        FreePlayBindingRegistry.MarkCardFreeNextPlay(created);

        await CardPileCmd.AddGeneratedCardToCombat(
            created, PileType.Hand, player, CardPilePosition.Random);
    }
}
