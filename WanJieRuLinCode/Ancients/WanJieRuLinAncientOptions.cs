using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
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
        // 规则一：「墨染江山」—— 加一张先古卡。
        var moRanRule = ModAncientOptionRule.Single(
            optionFactory: BuildMoRanOption,
            condition: null,
            priority: 0,
            skipDuplicateTextKeys: true);

        // 规则二：「金丝鬼墨」—— 把起始遗物鬼墨换成它的升级形态。
        //
        // ★ 为什么要有这条：
        //   JinSiGuiMo 原先注册进了遗物池、稀有度是 RelicRarity.Starter，
        //   但**全工程没有任何地方发放它**（既不是 RegisterCharacterStarterRelic，
        //   也没有事件/先古引用）。而 Starter 稀有度通常不会出现在奖励池里，
        //   所以它实际上是玩家永远拿不到的废内容。
        //   这里给它一条正经获取途径，同时它「鬼墨的升级形态」的定位也就成立了。
        //
        // condition 保证：只有身上真的有「鬼墨」时才出现这个选项
        //   （万一先古在别的时机触发、或玩家已换过，就不会给出一个空选项）。
        var jinSiRule = ModAncientOptionRule.Single(
            optionFactory: BuildJinSiGuiMoOption,
            condition: ancient => HasGhostQiRelic(ancient.Owner),
            priority: 1,
            skipDuplicateTextKeys: true);

        // Neow —— 序幕先古。
        ModAncientOptionRegistry.Register<Neow>(Entry.ModId, moRanRule);
        ModAncientOptionRegistry.Register<Neow>(Entry.ModId, jinSiRule);

        // Darv —— 达佛。
        ModAncientOptionRegistry.Register<Darv>(Entry.ModId, moRanRule);
        ModAncientOptionRegistry.Register<Darv>(Entry.ModId, jinSiRule);
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
        // 注意：这里不能再 new WanJieRuLinAncient()。
        // ModCardTemplate 的构造函数会把模型注册进 ModelDb，
        // 而该卡已由 [RegisterCard] 注册过，重复实例化会抛 DuplicateModelException，
        // 导致先古选项整体构造失败（选项不显示）。
        // 正确做法是取 ModelDb 里已经注册好的规范实例来读文案。
        var canonical = ModelDb.Card<WanJieRuLinAncient>();

        return new EventOption(
            ancient,
            onChosen: () => GrantWanJieRuLinAncient(ancient),
            // 标题仍可复用卡牌标题（纯文本，不含占位符）。
            title: canonical.TitleLocString,
            // 描述必须用独立键：见类注释里关于事件界面变量表的说明。
            description: new LocString(LocTable, MoRanOptionTextKey + ".description"),
            textKey: MoRanOptionTextKey,
            hoverTips: []);
    }

    /// <summary>
    /// 把「墨染江山」永久加入事件所属玩家的牌组，**并推进事件**。
    ///
    /// ⚠️ 必须推进！这是第十二轮踩的第二个坑（第一个是造牌登记）：
    ///
    /// 先古事件（<c>AncientEventModel</c>）的选项列表是**一次性快照**——
    /// <c>GenerateInitialOptionsWrapper()</c> 把结果缓存在 <c>GeneratedOptions</c> 字段里，
    /// <c>SetInitialEventState</c> 再把它 AddRange 进 <c>_currentOptions</c>。
    /// 此后**框架不会自动重建**这个列表。
    ///
    /// 而「事件能不能继续」的唯一信号源是 <c>EventModel.SetEventState</c> 末尾的
    /// <c>StateChanged?.Invoke(this)</c>。原版每个先古选项都在自己的 onChosen 里
    /// 调用 <c>SetEventState</c>（翻页）或 <c>SetEventFinished</c>（终局），从而触发推进。
    ///
    /// 我们如果只加牌、不推进，就会：
    ///   ① <c>BeforeOptionChosen</c> 已经把所有按钮 DisableOptionButtons 禁用（防连点，正常设计）；
    ///   ② <c>IsFinished</c> 仍是 false ⇒ <c>NEventRoom.SetOptions</c> 不会生成「继续 / PROCEED」按钮；
    ///   ③ <c>StateChanged</c> 从未触发 ⇒ <c>EventRoom.OnEventStateChanged</c> 从不运行
    ///      ⇒ <c>MarkPreFinished</c> / 存档 都不发生。
    /// 界面表现就是「选项还在、点不动、也没有继续按钮」= 软锁。
    ///
    /// 所以加完牌后**必须**自己调 <see cref="EventModel.SetEventFinished"/>。
    /// 它的 IL 是：<c>SetEventState(desc, [])</c> → 选项数为 0 ⇒ <c>_isFinished = true</c>
    /// → <c>StateChanged</c> 触发 → <c>NEventRoom.SetOptions</c> 看到 IsFinished
    /// → 生成 PROCEED 按钮 → 玩家点击 → <c>NEventRoom.Proceed()</c> → 回地图。
    /// </summary>
    private static async Task GrantWanJieRuLinAncient(AncientEventModel ancient)
    {
        // ★ 无论成败都必须推进事件，否则软锁。用 try/finally 兜住所有分支
        //   （包括造牌抛异常的情况 —— onChosen 由 TaskHelper.RunSafely 调用，
        //    异常会被吞掉，玩家只会看到「点了没反应」）。
        try
        {
            if (ancient.Owner is not { } player)
            {
                Entry.Logger.Warn("[MoRan] 事件没有 Owner，无法加牌；仍推进事件以免软锁。");
                return;
            }

            // ★ 造牌四连（每一步都由 IL 反编译确认，缺一步就崩）：
            //
            // ① `ModelDb.Card<T>()` 给的是**规范（canonical / 不可变）**实例，不能直接改。
            //
            // ② **手工 `CreateCloneForPlayer(player)` 是错的！**
            //    它的 IL 只有两句：`CreateClone()` + `_owner = player`，**从不把牌放进任何堆**。
            //    而 `CreateClone()` 一开头就读 `get_Pile()`：
            //        IL_0001: call   CardModel.get_Pile
            //        IL_0006: brfalse.s       ← Pile == null 就跳进异常分支
            //        IL_000E: callvirt CardPile.get_Type   ← ★ NullReferenceException 就炸在这
            //    对一张「还没有堆」的新牌调用它必然 NRE。
            //
            // ③ **只 `ToMutable() + 设 Owner` 仍然不够**（第九轮就是死在这）。
            //    `CardPileCmd.Add` 在 `PileType.Deck` 上有一条**独立守卫**
            //    （`CardPileCmd+<Add>d__10.MoveNext`，IL_0164~IL_01D9）：
            //        IL_016F: ldc.i4.6                      ← PileType.Deck == 6
            //        IL_0170: bne.un.s -> IL_01DA           ← 不是 Deck 就跳过本段
            //        IL_0179: card.Owner.RunState
            //        IL_0185: IRunState.ContainsCard(card)  ← 必须为 true，否则……
            //        IL_01CA: ldstr " must be added to a RunState before adding it to your deck."
            //        IL_01D9: throw InvalidOperationException
            //    而 `RunState.ContainsCard` 的实现是
            //        IL_0001: ldfld RunState._allCards
            //        IL_0007: callvirt List`1.Contains(CardModel)
            //    —— **引用相等**。`ToMutable()` 出来的克隆从未登记进 `_allCards`，
            //    所以守卫必然抛「must be added to a RunState」，事件卡死。
            //    （另有一条 CombatState 守卫，在 Deck 之外的分支，同理。）
            //
            // ④ 正解 = **`RunState.CreateCard(canonical, player)`**，它一步到位做了三件事
            //    （`RunState.CreateCard(CardModel,Player)` 的 IL，逐句可对照）：
            //        IL_0001: CardModel.ToMutable()           ① 拿可变实例
            //        IL_000A: RunState.AddCard(card, owner)   ② 设 Owner **并登记进 _allCards**
            //        IL_0010: CardModel.AfterCreated()        ③ 收尾钩子
            //        IL_0016: ret                             ④ 返回可用的实例
            //    `RunState.AddCard(card, owner)` 内部：
            //        IL_000A: CardModel.set_Owner(Player)     ← 设 Owner
            //        IL_0011: RunState.AddCard(card)          → IL_005D: _allCards.Add(card)
            //    这就是「先登记进 RunState」的唯一官方入口。
            //    拿到登记好的牌后再 `CardPileCmd.Add` 入堆，守卫即可通过。
            //
            // 官方范本（战斗外把牌加进 Deck）——`EventModel.SelectCardsToAddToDeckFromGrid`
            // 的状态机就是这么写的：`CardPileCmd.Add(card, Deck, Bottom, null, false)`，
            // 而它的 card 早已登记在 RunState 里 —— 这正是我们此前缺掉的一环。
            //
            // 附：`RelicModel` 侧的官方写法（`EventOption.WithRelic` 的 IL）确实是
            //     `ToMutable()` → `set_Owner(player)` → 交给框架，**但它不需要登记**，
            //     因为遗物没有「必须属于某 RunState」的入堆守卫。
            //     把卡牌照搬遗物的写法（第九轮犯的错）就会撞上上面那条守卫。
            // ★ 注意 `Player.RunState` 的**静态类型是 `IRunState`**（接口），
            //   而 `CreateCard` 只定义在**具体类 `RunState`** 上（`IRunState` 上没有）。
            //   所以必须先向下转型，否则编译不过（CS1061）。
            //
            // ⚠️ 实测注意：改完这个文件后**必须完全退出游戏再重开**。
            //   RitsuLib 只在启动时加载 mod DLL，游戏进程未退出时重新 build
            //   虽然会把新 DLL 写到 mods 目录，但**运行中的进程仍用旧代码**，
            //   于是会看到「明明改了却还是同样的异常」的假象（本 bug 因此被误判过一轮）。
            if (player.RunState is not RunState runState)
            {
                Entry.Logger.Warn("[MoRan] RunState 不是具体 RunState，无法加牌；仍推进事件以免软锁。");
                return;
            }

            // ★ ① 先让框架在 RunState 里「造」出这张牌（= ToMutable + 设 Owner + 登记 + AfterCreated）。
            var card = runState.CreateCard(ModelDb.Card<WanJieRuLinAncient>(), player);

            // ★ ② 再 permanent 加入主牌组（Deck 堆）。守卫此时检查 ContainsCard → 通过。
            await CardPileCmd.Add(card, PileType.Deck, CardPilePosition.Bottom, null, false);
        }
        finally
        {
            // ★ ③ 无论如何都要推进事件，否则先古会停在「选项还在、按钮已禁用」的软锁状态。
            //
            // ⚠️ 这里**不能**用 `EventModel.SetEventFinished(...)` —— 它是 **protected**（`Family`），
            //   mod 里根本没有权限调用（编译不过）。
            //   实测可见性（IL 探针 16 确认）：
            //     EventModel.SetEventFinished(LocString)        → Family（protected）
            //     EventModel.SetEventState(LocString, options)  → Family（protected）
            //     AncientEventModel.Done()                      → Family（protected）
            //     AncientEventModel.StartPreFinished()          → ★ Public ★
            //
            // 正解 = **`AncientEventModel.StartPreFinished()`**（public，官方语义就是「结束先古」）：
            //     IL_0001: if (CustomDonePage != null) goto IL_002A
            //     IL_0008: L10NLookup(Id.Entry + ".pages.DONE.description")
            //     IL_0024: SetEventFinished(那一页文案)        ← 内部自己调 protected 方法
            //     IL_003C: ret
            //   它内部把「取结语文案 + 调 SetEventFinished」全做了，是 mod 唯一合法入口。
            //   文案键 `<先古Id>.pages.DONE.description` 是原版资源，Neow / Darv 都有。
            //
            // 走完之后：`SetEventFinished` → `SetEventState(desc, [])` → 选项数 0 ⇒ `_isFinished = true`
            //   → `StateChanged` 触发 → `EventRoom.OnEventStateChanged` → `MarkPreFinished` + 存档
            //   → `NEventRoom.SetOptions` 看到 IsFinished → 生成 PROCEED 按钮
            //   → 玩家点「继续」→ `NEventRoom.Proceed()` → `NMapScreen.Open(false)` → 回地图。
            ancient.StartPreFinished();
        }
    }

    // ==================================================================
    // 规则二：「金丝鬼墨」—— 起始遗物的升级形态
    // ==================================================================

    /// <summary>
    /// 「金丝鬼墨」选项的文案键。
    /// ⚠️ 同样**不允许出现任何 {占位符}** —— 事件界面拿不到遗物的 DynamicVars。
    /// </summary>
    private const string JinSiOptionTextKey = "WAN_JIE_RU_LIN_RELIC_JIN_SI_GUI_MO_OPTION";

    /// <summary>
    /// 判断玩家身上是否还带着未升级的「鬼墨」。
    ///
    /// ★ 包一层 try/catch 是必要的防御：`Player.GetRelic&lt;T&gt;()` 在**没有该遗物**
    ///   时的行为没有文档保证（可能返回 null，也可能抛）。而这个判断会被
    ///   `ModAncientOptionRegistry.ShouldApply` 在**每个先古事件**上调用 ——
    ///   一旦抛出就会连累整个先古事件的选项生成。
    ///   典型触发场景：玩家上一处先古已经换成了「金丝鬼墨」，到下一处先古时
    ///   身上就没有「鬼墨」了。
    /// </summary>
    private static bool HasGhostQiRelic(Player? player)
    {
        if (player is null)
        {
            return false;
        }

        try
        {
            return player.GetRelic<Relics.GuiMo>() is not null;
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn("[JinSiGuiMo] 查询鬼墨遗物时抛异常，按「没有」处理：" + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 构造「金丝鬼墨」选项：把身上的「鬼墨」换成「金丝鬼墨」。
    /// </summary>
    private static EventOption BuildJinSiGuiMoOption(AncientEventModel ancient)
    {
        return new EventOption(
            ancient,
            onChosen: () => GrantJinSiGuiMo(ancient),
            title: new LocString(LocTable, JinSiOptionTextKey + ".title"),
            description: new LocString(LocTable, JinSiOptionTextKey + ".description"),
            textKey: JinSiOptionTextKey,
            hoverTips: []);
    }

    /// <summary>
    /// 用 <c>RelicCmd.Replace</c> 把玩家身上的「鬼墨」替换成「金丝鬼墨」。
    ///
    /// 为什么用 Replace 而不是「先 Remove 再 Obtain」：
    /// - `RelicCmd.Replace(RelicModel original, RelicModel replace)` 是原版自带的
    ///   原子操作（签名由探针 21 从 sts2.dll 反射确认），一次调用完成替换，
    ///   不用自己处理「移除失败但获得了新的」这种中间态。
    /// - `original` 必须是玩家**实际持有**的实例（用 `Player.GetRelic&lt;T&gt;()` 取），
    ///   不能传 `ModelDb` 的规范实例 —— 规范实例不在玩家的遗物列表里。
    /// - `replace` 传 `ModelDb.Relic&lt;JinSiGuiMo&gt;()` 的规范实例即可，
    ///   与 `RelicCmd.Obtain` 的用法一致（Obtain 内部会处理可变副本与归属）。
    ///
    /// ⚠️ 与「墨染江山」一样，无论成败都必须推进事件，否则会软锁。
    ///    （onChosen 由 TaskHelper.RunSafely 调用，异常会被吞掉，
    ///      玩家只会看到「点了没反应」。）
    /// </summary>
    private static async Task GrantJinSiGuiMo(AncientEventModel ancient)
    {
        try
        {
            if (ancient.Owner is not { } player)
            {
                Entry.Logger.Warn("[JinSiGuiMo] 事件没有 Owner，无法替换遗物；仍推进事件以免软锁。");
                return;
            }

            if (player.GetRelic<Relics.GuiMo>() is not { } owned)
            {
                Entry.Logger.Warn("[JinSiGuiMo] 玩家身上没有鬼墨，跳过替换；仍推进事件以免软锁。");
                return;
            }

            // ★ 修复（2026-09-28）：原写法 `RelicCmd.Replace(owned, ModelDb.Relic<JinSiGuiMo>())`
            //   直接传**规范（canonical / 不可变）实例** → 线上崩：
            //     CanonicalModelException: Canonical model of type JinSiGuiMo used in incorrect place.
            //       at AbstractModel.AssertMutable()
            //       at RelicCmd.Replace(RelicModel original, RelicModel replace)
            //   即 RelicCmd.Replace **要求 replace 是可变的**，与下面原注释的说法相反。
            //   （onChosen 由 TaskHelper.RunSafely 调用，异常被吞 → 玩家只看到「点了没反应」，
            //     并且事件未推进 → 整局卡死、后续战斗不发牌。）
            //
            //   正确姿势与卡片造牌一致：**ToMutable() → 设 Owner → 交给框架**。
            var replacement = ModelDb.Relic<Relics.JinSiGuiMo>().ToMutable();
            replacement.Owner = player;
            await RelicCmd.Replace(owned, replacement);
        }
        finally
        {
            ancient.StartPreFinished();
        }
    }
}
