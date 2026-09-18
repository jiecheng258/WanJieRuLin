using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using STS2RitsuLib.Scaffolding.Ancients.Options;
using WanJieRuLin.Cards;

namespace WanJieRuLin.Ancients;

/// <summary>
/// 万界如林的先古之民选项注入。
///
/// 设计决定（用户确认）：**复用原版先古之民**，不新增独立先古 NPC。
/// 具体做法是把本模组的先古卡「墨染江山」作为额外选项，
/// 追加到原版先古之民的初始选项列表里。
///
/// 覆盖范围：Neow（序幕）与 Darv（达佛）。
/// 这两个先古恰好在本模组的 ancients.json 里写了「林」的专属对白，
/// 因此注入选项不会出现「有选项、没对话」的割裂感。
///
/// 关于文案（踩坑记录）：
/// **不能**直接把卡牌自己的 Description 拿来当选项描述。
/// 事件界面（NEventOptionButton._Ready）渲染选项文案时只注入事件变量
/// （character / pronoun / IsMultiplayer 这几个），**拿不到卡牌的 DynamicVars**；
/// 而卡牌描述里的 {Var} 占位符正是从 DynamicVars 解析的。
/// 于是选项一显示，日志就刷
/// "No source extension could handle the selector named 'Bonus'"。
/// 所以选项用自己的文案键（表 cards，键见 <see cref="MoRanOptionTextKey"/>），
/// 文案里**不允许出现任何 {占位符}**。
/// </summary>
public static class WanJieRuLinAncientOptions
{
    /// <summary>
    /// 把「墨染江山」选项注册到原版先古之民的初始选项列表。
    /// 在 <see cref="Entry.Initialize"/> 中调用。
    /// </summary>
    public static void Register()
    {
        var rule = ModAncientOptionRule.Single(
            optionFactory: BuildMoRanOption,
            condition: null,
            priority: 0,
            skipDuplicateTextKeys: true);

        // Neow —— 序幕先古。
        ModAncientOptionRegistry.Register<Neow>(Entry.ModId, rule);

        // Darv —— 达佛。
        ModAncientOptionRegistry.Register<Darv>(Entry.ModId, rule);
    }

    /// <summary>
    /// 选项的文本键根。占位符审计脚本会检查这个键下的文案不含 <c>{…}</c>。
    /// </summary>
    private const string MoRanOptionTextKey = "WAN_JIE_RU_LIN_CARD_MO_RAN_JIANG_SHAN_OPTION";

    /// <summary>选项文案所在的本地化表。用 cards 表（卡牌文案都走这里，必定能解析到）。</summary>
    private const string LocTable = "cards";

    /// <summary>构造「墨染江山」选项：把一张墨染江山永久加入牌组。</summary>
    private static EventOption BuildMoRanOption(AncientEventModel ancient)
    {
        // 注意：这里不能再 new MoRanJiangShan()。
        // ModCardTemplate 的构造函数会把模型注册进 ModelDb，
        // 而该卡已由 [RegisterCard] 注册过，重复实例化会抛 DuplicateModelException，
        // 导致先古选项整体构造失败（选项不显示）。
        // 正确做法是取 ModelDb 里已经注册好的规范实例来读文案。
        var canonical = ModelDb.Card<MoRanJiangShan>();

        return new EventOption(
            ancient,
            onChosen: () => GrantMoRanJiangShan(ancient),
            // 标题仍可复用卡牌标题（纯文本，不含占位符）。
            title: canonical.TitleLocString,
            // 描述必须用独立键：见类注释里关于事件界面变量表的说明。
            description: new LocString(LocTable, MoRanOptionTextKey + ".description"),
            textKey: MoRanOptionTextKey,
            hoverTips: []);
    }

    /// <summary>把「墨染江山」永久加入事件所属玩家的牌组。</summary>
    private static async Task GrantMoRanJiangShan(AncientEventModel ancient)
    {
        if (ancient.Owner is not { } player)
        {
            return;
        }

        // 取规范实例再克隆给玩家（同样避免重复 new 触发注册冲突）。
        var card = ModelDb.Card<MoRanJiangShan>().CreateCloneForPlayer(player);

        // 永久加入主牌组（Deck 堆）。
        await CardPileCmd.Add(card, PileType.Deck, CardPilePosition.Bottom, null, false);
    }
}
