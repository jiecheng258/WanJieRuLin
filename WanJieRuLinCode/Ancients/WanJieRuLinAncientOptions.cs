using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Ancients.Options;
using WanJieRuLin.Cards;

namespace WanJieRuLin.Ancients;

/// <summary>
/// 万界如林的先古之民选项注入。
///
/// 设计决定（沿用旧版，用户确认）：**复用原版先古之民**，不新增独立先古 NPC。
/// 把本模组的 4 张先古卡（鬼墨真言 / 先古之兆 / 墨域 / 万界来朝）作为额外选项，
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
/// 所以选项用自己的文案键（表 cards，键见 <see cref="OptionTextKey"/>），
/// 文案里**不允许出现任何 {占位符}**。
///
/// 关于造牌（踩坑记录）：先古选项发牌进牌组必须走
/// <c>RunState.CreateCard</c>（一次完成「可变 + 设 Owner + 登记进 RunState」），
/// 再 <c>CardPileCmd.Add(..., PileType.Deck, ...)</c>。
/// 且 onChosen 必须 <c>finally { ancient.StartPreFinished(); }</c> 推进事件，
/// 否则软锁（第十二轮）。
/// </summary>
public static class WanJieRuLinAncientOptions
{
    /// <summary>
    /// 把 4 张先古卡选项注册到原版先古之民的初始选项列表。
    /// 在 <see cref="Entry.Initialize"/> 中调用。
    /// </summary>
    public static void Register()
    {
        // 每张先古卡一个选项，互不覆盖。
        RegisterCardOption<GuiMoZhenYan>();
        RegisterCardOption<XianGuZhiZhao>();
        RegisterCardOption<MoYu>();
        RegisterCardOption<WanJieLaiChao>();
    }

    /// <summary>为单张先古卡注册一条「获得该卡」的选项规则。</summary>
    private static void RegisterCardOption<TCard>() where TCard : WanJieRuLinCardModel
    {
        var rule = ModAncientOptionRule.Single(
            optionFactory: ancient => BuildCardOption<TCard>(ancient),
            condition: null,
            priority: 0,
            skipDuplicateTextKeys: true);

        // Neow —— 序幕先古。
        ModAncientOptionRegistry.Register<Neow>(Entry.ModId, rule);

        // Darv —— 达佛。
        ModAncientOptionRegistry.Register<Darv>(Entry.ModId, rule);
    }

    /// <summary>选项文案所在的本地化表。用 cards 表（卡牌文案都走这里，必定能解析到）。</summary>
    private const string LocTable = "cards";

    /// <summary>选项文本键根（表 cards，不含占位符）。</summary>
    private static string OptionTextKey<TCard>() where TCard : WanJieRuLinCardModel =>
        $"WAN_JIE_RU_LIN_CARD_{UpperSnake(typeof(TCard).Name)}_OPTION";

    private static string UpperSnake(string name)
    {
        // 在大写前插入下划线（复用审计脚本的规则）。
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            char ch = name[i];
            if (i > 0 && char.IsUpper(ch))
            {
                char prev = name[i - 1];
                char nxt = i + 1 < name.Length ? name[i + 1] : '\0';
                if (char.IsLower(prev) || (char.IsUpper(prev) && char.IsLower(nxt)))
                    sb.Append('_');
            }
            sb.Append(char.ToUpper(ch));
        }
        return sb.ToString();
    }

    /// <summary>构造「获得某张先古卡」的选项。</summary>
    private static EventOption BuildCardOption<TCard>(AncientEventModel ancient)
        where TCard : WanJieRuLinCardModel
    {
        // 注意：这里不能再 new TCard()。
        // ModCardTemplate 的构造函数会把模型注册进 ModelDb，
        // 而该卡已由 [RegisterCard] 注册过，重复实例化会抛 DuplicateModelException，
        // 导致先古选项整体构造失败（选项不显示）。
        // 正确做法是取 ModelDb 里已经注册好的规范实例来读文案。
        var canonical = ModelDb.Card<TCard>();

        return new EventOption(
            ancient,
            onChosen: () => GrantCard<TCard>(ancient),
            // 标题仍可复用卡牌标题（纯文本，不含占位符）。
            title: canonical.TitleLocString,
            // 描述必须用独立键：见类注释里关于事件界面变量表的说明。
            description: new LocString(LocTable, OptionTextKey<TCard>() + ".description"),
            textKey: OptionTextKey<TCard>(),
            hoverTips: []);
    }

    /// <summary>把某张先古卡永久加入事件所属玩家的牌组。</summary>
    private static async Task GrantCard<TCard>(AncientEventModel ancient)
        where TCard : WanJieRuLinCardModel
    {
        try
        {
            if (ancient.Owner is not { } player)
            {
                Entry.Logger.Warn($"[Ancient] {typeof(TCard).Name}: ancient.Owner is null");
                return;
            }

            // ★ 战斗外造牌唯一正解：RunState.CreateCard 一次完成
            //   ① 可变 ② 设 Owner ③ 登记进 RunState 名册。
            if (player.RunState is not RunState runState)
            {
                Entry.Logger.Warn($"[Ancient] {typeof(TCard).Name}: RunState is null");
                return;
            }

            var card = runState.CreateCard(ModelDb.Card<TCard>(), player);

            // 永久加入主牌组（Deck 堆）。
            await CardPileCmd.Add(card, PileType.Deck, CardPilePosition.Bottom, null, false);
        }
        finally
        {
            // ★ 无论如何都要推进，否则软锁（早退 / 异常路径也覆盖）。
            ancient.StartPreFinished();
        }
    }
}
