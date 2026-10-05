using System.Collections.Generic;

namespace WanJieRuLin.Powers;

/// <summary>
/// 面牌回流记录表 —— 「去年今日此门中」：**每张面牌每场战斗只能回流一次**。
///
/// 用「玩家 id + 卡牌类型」做键；每场战斗开始时由
/// <see cref="FaceReturnResetPower"/> 清空。
/// </summary>
public static class WanJieFaceReturn
{
    private static readonly HashSet<string> Used = [];

    private static string Key(object player, Type cardType)
        => player.GetHashCode() + "|" + cardType.FullName;

    /// <summary>本场战斗这张面牌是否还没回流过（并且把它标记为已用）。</summary>
    public static bool TryClaim(object player, Type cardType)
        => Used.Add(Key(player, cardType));

    /// <summary>开战清空。</summary>
    public static void Reset() => Used.Clear();
}
