using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 剑 —— 飘渺剑诀 / 剑！/ 一把小剑 共用的随机剑效果。
/// 金剑：回合开始 +3 活力；木剑：回合开始回复 1 生命；
/// 水剑：弃牌堆一张攻击牌获得消耗 + 单回合保留；火剑：回合开始 +1 临时力量；土剑：回合开始 +1 覆甲。
///
/// 重要（踩坑）：
/// 必须走 <see cref="PowerCmd.Apply{TPower}"/> 的 <b>泛型</b> 重载。之前踩过两个坑：
/// 1. <c>new JinJianPower()</c> —— 新建实例没有 ModelId，框架叠加 / 触发 hook
///    时找不到它，表现为「剑拿到了但完全没效果」。
/// 2. <c>PowerCmd.Apply(ctx, ModelDb.Get(Type) as PowerModel, ...)</c> ——
///    ModelDb 返回的是 <b>规范实例（canonical / immutable）</b>，
///    而 PowerCmd.Apply 内部会 AssertMutable() 直接抛 CanonicalModelException，
///    把整个回合循环打死（表现为「卡死、打不出牌」）。
///
/// 泛型重载会自己解析规范模型并派生出可变实例，是唯一正确的用法。
/// </summary>
public static class Swords
{
    /// <summary>五把剑的能力类型原型（按索引对应 <see cref="Grant"/> 的 index）。</summary>
    public static readonly Type[] AllTypes =
    [
        typeof(JinJianPower),
        typeof(MuJianPower),
        typeof(ShuiJianPower),
        typeof(HuoJianPower),
        typeof(TuJianPower)
    ];

    /// <summary>取第 index 把剑对应的能力类型（索引自动取模）。</summary>
    public static Type GetTypeAt(int index)
    {
        var i = ((index % AllTypes.Length) + AllTypes.Length) % AllTypes.Length;
        return AllTypes[i];
    }

    /// <summary>随机获得一把剑的效果。</summary>
    public static Task<PowerModel?> GrantRandom(
        PlayerChoiceContext choiceContext, Creature owner, CardModel? cardSource) =>
        Grant(choiceContext, owner, cardSource, Random.Shared.Next(AllTypes.Length));

    /// <summary>
    /// 获得指定的剑效果（索引见 <see cref="AllTypes"/>）。
    /// 用 switch 显式派发到泛型重载，保证框架拿到的是可变实例。
    /// </summary>
    public static Task<PowerModel?> Grant(
        PlayerChoiceContext choiceContext, Creature owner, CardModel? cardSource, int index)
    {
        var i = ((index % AllTypes.Length) + AllTypes.Length) % AllTypes.Length;

        return i switch
        {
            0 => ApplyPower<JinJianPower>(choiceContext, owner, cardSource),
            1 => ApplyPower<MuJianPower>(choiceContext, owner, cardSource),
            2 => ApplyPower<ShuiJianPower>(choiceContext, owner, cardSource),
            3 => ApplyPower<HuoJianPower>(choiceContext, owner, cardSource),
            _ => ApplyPower<TuJianPower>(choiceContext, owner, cardSource),
        };
    }

    private static async Task<PowerModel?> ApplyPower<TPower>(
        PlayerChoiceContext choiceContext, Creature owner, CardModel? cardSource)
        where TPower : PowerModel
    {
        return await PowerCmd.Apply<TPower>(choiceContext, owner, 1m, owner, cardSource, false);
    }
}
