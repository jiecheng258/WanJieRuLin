using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

// ============================================================================
// 「本回合限定」能力基类
//
// ⚠️ 这是个必须显式处理的坑：塔2 **没有**「存活 N 回合」的堆叠类型。
//    PowerStackType 只有三个值：None / Counter / Single。
//    也就是说，一个普通能力一旦被施加，就会一直挂在身上（直到战斗结束），
//    框架**不会**因为文案里写了「本回合」就自动把它撤掉。
//
// 曾经踩过的坑：神圣暴走 / 尸瞳 / 傲慢 / 暂避锋芒 四个能力的文案都写
// 「本回合……」，但实现里没有任何移除逻辑 —— 实际效果是**整场战斗永久生效**。
// 后果举例：
//   - 神圣暴走：本回合攻击伤害翻倍 → 变成整场翻倍，鬼气只要够就一直双倍
//   - 尸瞳：本回合每打一张牌 +1 鬼气 → 永久生效，第 3 张牌之后就是无限鬼气
//   - 傲慢：本回合伤害等量转鬼气 → 永久 1:1 转换（鬼气在本模组里约等于能量）
//   - 暂避锋芒：本回合无法打出攻击牌 → 永久不能打攻击牌（负面效果也永久）
//
// 正确做法就是本基类：在**自己这一方**的回合结束时把自己移除。
// 注意 AfterSideTurnEnd 对双方回合结束都会触发，所以必须用
// participants 判断「这次结束的是不是我这一方」，否则敌方回合结束就把自己撤了。
//
// 需要「本回合生效」的能力请继承本类，不要继承 ModPowerTemplate。
// ============================================================================

/// <summary>
/// 只持续到「自己这一方本次回合结束」的能力。
///
/// 用法：<c>public sealed class XxxPower : WanJieTurnScopedPower</c>
/// 然后正常实现效果钩子即可，不需要自己写移除逻辑。
/// </summary>
public abstract class WanJieTurnScopedPower : ModPowerTemplate
{
    /// <summary>
    /// 自己这一方的回合结束时，把自己移除。
    /// </summary>
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        // 双方回合结束都会走这个钩子：只处理「参与者里包含自己」的那一次。
        if (Owner is { } owner && participants.Contains(owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}
