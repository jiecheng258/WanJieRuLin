using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;

namespace WanJieRuLin;

/// <summary>
/// 鬼气的读写封装。所有卡牌 / 遗物都通过这里操作鬼气，
/// 便于统一处理"获得鬼气 / 消耗鬼气"以及相关能力（鬼影森森、吞噬、化神）的触发。
/// </summary>
public static class GhostQi
{
    public static string Id => ModResources.GhostQiId;

    /// <summary>读取当前鬼气。</summary>
    public static int Get(Player player) => SecondaryResourceCmd.Get(player, Id);

    public static int Get(Creature? creature) => creature?.Player is { } p ? Get(p) : 0;

    /// <summary>读取当前上限（本模组是软上限，恒为 GhostQiSoftCap）。</summary>
    public static int? GetMax(Player player) => SecondaryResourceCmd.GetMax(player, Id);

    /// <summary>获得鬼气。会经过 ISecondaryResourceHookListener 的获得修正。</summary>
    public static Task Gain(Player player, int amount) =>
        amount <= 0 ? Task.CompletedTask : SecondaryResourceCmd.Gain(player, Id, amount);

    /// <summary>失去鬼气。</summary>
    public static Task Lose(Player player, int amount) =>
        amount <= 0 ? Task.CompletedTask : SecondaryResourceCmd.Lose(player, Id, amount);

    /// <summary>消耗鬼气。不足时不会扣除并返回 false（费用由框架校验，这里用于卡牌效果）。</summary>
    public static Task<bool> Spend(Player player, int amount)
    {
        if (amount <= 0)
        {
            return Task.FromResult(true);
        }

        return SecondaryResourceCmd.Spend(player, Id, amount);
    }

    /// <summary>直接设为指定数值。</summary>
    public static Task Set(Player player, int amount) => SecondaryResourceCmd.Set(player, Id, amount);

    /// <summary>是否足够支付。</summary>
    public static bool Has(Player player, int amount) => Get(player) >= amount;

    /// <summary>把玩家当前鬼气清空（"厉鬼复苏""我不玩了"等使用）。</summary>
    public static async Task Clear(Player player)
    {
        var cur = Get(player);
        if (cur > 0)
        {
            await Lose(player, cur);
        }
    }

    /// <summary>把当前鬼气全部消耗掉并返回实际消耗量。</summary>
    public static async Task<int> SpendAll(Player player)
    {
        var cur = Get(player);
        if (cur <= 0)
        {
            return 0;
        }

        await Spend(player, cur);
        return cur;
    }
}
