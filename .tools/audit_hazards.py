# -*- coding: utf-8 -*-
"""静态危险模式扫描 —— 把线上日志暴露的崩溃模式固化成检查项。

针对的是「编译能过、但进游戏必崩 / 卡死」的写法。
每一项都对应一个真实发生过的线上 bug（见 2026-09-15 工作日志）。

用法：
    python audit_hazards.py            # 扫描并打印
    python audit_hazards.py --self-test  # 顺带验证规则本身能命中已知坏例子
"""
import io, os, re, sys

CODE = r'C:\Users\wangx\Documents\Default Project\WanJieRuLin\WanJieRuLinCode'

# 我们自己的模型基类：派生自这些类型的类，实例必须从 ModelDb 取。
MODEL_BASE_PAT = re.compile(
    r'class\s+(\w+)\s*:\s*(?:[\w\.]*\.)?'
    r'(ModCardTemplate|ModPowerTemplate|ModTemporaryPowerTemplate|ModTemporaryAppliedPowerTemplate|'
    r'ModRelicTemplate|ModPotionTemplate|WanJieRuLinCardModel|WanJieRuLinRelic|'
    r'CardModel|PowerModel|RelicModel|PotionModel)\b'
)

RULES = [
    (
        'LocString.KeyPathToLocString',
        re.compile(r'LocString\s*\.\s*KeyPathToLocString\s*\('),
        '运行期本地化表未必就绪 → IndexOutOfRangeException，会打死回合循环（表现为卡死、打不出牌）。\n'
        '        改用原版自带的 CardSelectorPrefs.XxxSelectionPrompt，或 new LocString(table, key)。',
    ),
    (
        'PowerCmd.Apply(...) 非泛型重载',
        re.compile(r'PowerCmd\s*\.\s*Apply\s*\(\s*[^<)]'),
        '非泛型重载要求传入「可变实例」。传 ModelDb 返回的规范实例（immutable）会触发\n'
        '        AssertMutable() → CanonicalModelException，直接卡死战斗。\n'
        '        改用泛型重载 PowerCmd.Apply<T>(choiceContext, owner, amount, applier, cardSource)。',
    ),
    (
        'new Random()',
        re.compile(r'\bnew\s+Random\s*\(\s*\)'),
        '建议改用 Random.Shared（线程安全）。非致命。',
    ),
]

# ★ 规范实例直接克隆：ModelDb.Card<T>() / Activator.CreateInstance(...) 等拿到的
#   都是 canonical(不可变) 实例，直接 .CreateCloneForPlayer() / .CreateClone() 会在内部
#   走到 CreateClone() -> AssertMutable() 而抛 CanonicalModelException。
#   必须先 .ToMutable()。
#   这类异常发生在 EventOption.Chosen() / 遗物战斗开始 等流程里，
#   会让「点击之后的收尾流程」整段中断 —— 表现为【选了选项却无法继续下一步】。
#
# 匹配思路：捕获「一整条链式调用」中的克隆调用，只要它前面没有 ToMutable()
# 就报。分两步：
#   rx_raw    —— 找出含 .CreateClone 的语句
#   rx_ok     —— 该语句里出现 .ToMutable() 则放过
RULES_CHAIN = [
    (
        '凭空造牌时手工调用 CreateClone / CreateCloneForPlayer / CreateDupe',
        re.compile(r'\.\s*(?:CreateClone(?:ForPlayer)?|CreateDupe)\s*\('),
        None,   # 无条件命中：不管前面有没有 ToMutable 都是错的
        '★ 造牌正解：**不要手工克隆**。\n'
        '        CreateCloneForPlayer 的 IL 只有 `CreateClone()` + `_owner = player`，\n'
        '        从**不把牌放进堆**；而 CreateClone() 一开头就读 `get_Pile()`，\n'
        '        对一张「还没有堆」的新牌必然 NullReferenceException。\n'
        '        CreateDupe 内部就是转调 CreateCloneForPlayer，同样会 NRE。\n'
        '        全程序集扫描确认：原版只有 6 处用它们，全部处于「牌已在堆里」的上下文\n'
        '        （如 ImitationLearningPower 对 CardPlay.Card 做副本）。\n'
        '        正确姿势 = 「ToMutable() → 设 Owner → 交给框架」：\n'
        '          · 战斗内：player.Creature.CombatState.CreateCard(canonical, player)\n'
        '          · 加进牌组：card.Owner = player; await CardPileCmd.Add(card, PileType.Deck, ...)\n'
        '          · 加进战斗：card.Owner = player; await CardPileCmd.AddGeneratedCardToCombat(card, ...)\n'
        '        参考原版 EventOption.WithRelic 的 IL：RelicModel.ToMutable()\n'
        '        → if (player != null) relic.set_Owner(player) → EventOption.WithRelic(relic)。',
    ),
]

# ★ 第九/十轮：**战斗外**（先古事件等）往 Deck 加牌时，只 ToMutable + 设 Owner 是不够的。
#
#   `CardPileCmd+<Add>d__10.MoveNext` 对 PileType.Deck 有一条独立守卫：
#       IL_016F: ldc.i4.6                       ← PileType.Deck == 6
#       IL_0170: bne.un.s -> IL_01DA            ← 不是 Deck 就跳过本段
#       IL_0179: card.Owner.RunState
#       IL_0185: IRunState.ContainsCard(card)   ← 必须为 true
#       IL_01CA: ldstr " must be added to a RunState before adding it to your deck."
#       IL_01D9: throw InvalidOperationException
#   而 `RunState.ContainsCard` = `_allCards.Contains(card)`，是**引用相等**。
#   `ToMutable()` 出来的克隆从未登记进 `_allCards` → 守卫必然抛异常 → 事件卡死。
#
#   正解 = `RunState.CreateCard(canonical, player)`，它一步做完三件事：
#       ToMutable() → RunState.AddCard(card, owner)（设 Owner **并登记进 _allCards**）
#                   → card.AfterCreated()
#   （`Player.RunState` 静态类型是 `IRunState`，接口上**没有** CreateCard，须转型到
#     `MegaCrit.Sts2.Core.Runs.RunState`。）
#
#   官方范本：`EventModel.SelectCardsToAddToDeckFromGrid` 的 `<>d__113.MoveNext`
#   IL_00F6: call CardPileCmd.Add(card, Deck, Bottom, null, false)
#   —— 它的 card 来自 CardCreationResult，早已登记在 RunState 里。
#
#   注意：这条**只针对 Deck**。加进 Draw/Hand/Discard 等战斗堆走另一条守卫
#   （`card.IsInCombat && CombatState != null` → 要求 `CombatState.ContainsCard`），
#   那种情况用 `CombatState.CreateCard` 或 `AddGeneratedCardToCombat` 即可。
RULES_DECK_RUNSTATE = [
    (
        '战斗外造牌加进 Deck，但只 ToMutable + 设 Owner（缺 RunState 登记）',
        # 该文件里出现「往 Deck 堆 Add」
        re.compile(r'CardPileCmd\s*\.\s*Add\s*\([^)]*PileType\s*\.\s*Deck'),
        # 同一个方法体内若出现 RunState.CreateCard / RunState.AddCard 则放行
        re.compile(r'(?:runState|RunState)\s*\.\s*(?:CreateCard|AddCard)\s*\('),
        '★ 战斗外往 Deck 加牌必须先「登记进 RunState」，否则 CardPileCmd.Add 抛\n'
        '        InvalidOperationException: "<CARD> must be added to a RunState before\n'
        '        adding it to your deck."（先古选项点了没反应/事件卡死的元凶）。\n'
        '        正解：\n'
        '          if (player.RunState is not RunState rs) return;   // 注意转型：\n'
        '          // Player.RunState 静态类型是 IRunState，接口上没有 CreateCard\n'
        '          var card = rs.CreateCard(ModelDb.Card<T>(), player);  // ToMutable+设Owner+登记\n'
        '          await CardPileCmd.Add(card, PileType.Deck, CardPilePosition.Bottom, null, false);\n'
        '        范本：原版 EventModel.SelectCardsToAddToDeckFromGrid。',
    ),
]


def is_comment(line):
    st = line.strip()
    return st.startswith('//') or st.startswith('///') or st.startswith('*') or st.startswith('/*')


# ★ 第八轮修正：原来这里认为「.ToMutable().CreateCloneForPlayer() 是好写法」，
#   已被 IL 反编译证伪 —— CreateCloneForPlayer 只设 _owner、从不入堆，
#   而 CreateClone() 一开头就读 get_Pile()，新牌（Pile == null）必 NRE。
#   所以现在**任何** CreateClone / CreateCloneForPlayer / CreateDupe 都直接报，
#   不再区分前面有没有 ToMutable。
def ok_chain(line: str, ok_rx=None) -> bool:
    """返回 True 表示该行可放行。

    ok_rx 为 None 时表示「无条件命中」——只要匹配到就报，不放行。
    """
    if ok_rx is None:
        return False
    return bool(ok_rx.search(line))


def collect_model_classes():
    """先扫一遍，收集本工程里所有派生自模型基类的类名。"""
    names = set()
    for dp, dn, fn in os.walk(CODE):
        for f in fn:
            if not f.endswith('.cs'):
                continue
            src = io.open(os.path.join(dp, f), encoding='utf-8', errors='replace').read()
            for m in MODEL_BASE_PAT.finditer(src):
                names.add(m.group(1))
    return names


def scan(model_classes):
    findings = []
    scanned = 0
    # new <自己定义的模型类>()
    new_model_rx = re.compile(
        r'\bnew\s+(' + '|'.join(sorted(map(re.escape, model_classes))) + r')\s*\(\s*\)'
    ) if model_classes else None

    for dp, dn, fn in os.walk(CODE):
        for f in sorted(fn):
            if not f.endswith('.cs'):
                continue
            scanned += 1
            p = os.path.join(dp, f)
            rel = os.path.relpath(p, CODE)
            lines = list(io.open(p, encoding='utf-8', errors='replace'))
            # ★ 文件级（方法级近似）判据：只要本文件里有 RunState.CreateCard/AddCard，
            #   就认为该文件的 Deck 写入已做过登记（本项目一处一个文件，够用）。
            file_has_runstate_register = any(
                rx_ok.search(l) for _, _, rx_ok, _ in RULES_DECK_RUNSTATE for l in lines
            )
            for n, line in enumerate(lines, 1):
                if is_comment(line):
                    continue
                for name, rx, hint in RULES:
                    if rx.search(line):
                        findings.append((rel, n, name, line.strip(), hint))
                for name, rx_raw, rx_ok, hint in RULES_CHAIN:
                    # ★ 第八轮修正：造牌时**任何**手工克隆都是错的，不再放行
                    #   .ToMutable().CreateCloneForPlayer(...) 这种「看起来对」的写法。
                    #   CreateCloneForPlayer 只设 _owner 不入堆，CreateClone() 一开头
                    #   读 get_Pile()，新牌必 NRE。只有 ok_rx 非 None 时才可能放行。
                    if not rx_raw.search(line):
                        continue
                    if ok_chain(line, rx_ok):
                        continue
                    findings.append((rel, n, name, line.strip(), hint))
                for name, rx_raw, rx_ok, hint in RULES_DECK_RUNSTATE:
                    if not rx_raw.search(line):
                        continue
                    if file_has_runstate_register:
                        continue
                    findings.append((rel, n, name, line.strip(), hint))
                if new_model_rx and new_model_rx.search(line):
                    findings.append((
                        rel, n, 'new <模型类>() 直接构造', line.strip(),
                        'AbstractModel 构造函数会向 ModelDb 注册 → DuplicateModelException。\n'
                        '        改用 ModelDb.Card<T>() / ModelDb.Power<T>() 再 '
                        '.CreateCloneForPlayer(player) / .ToMutable(n)。',
                    ))
    return findings, scanned


SELF_TEST_HIT = [
    'LocString.KeyPathToLocString("A.b"), 1);',
    'await ShuffleToDrawPile(new YinSenSen());',
    'var p = new JinJianPower();',
    'await PowerCmd.Apply(choiceContext, power, owner, 1m, owner, cardSource, false);',
    'var rng = new Random();',
    # ★ 造牌规则：手工克隆一律命中
    'var card = ModelDb.Card<MoRanJiangShan>().CreateCloneForPlayer(player);',
    'var created = canonical.CreateCloneForPlayer(player);',
    'var c = ModelDb.Card<YinSenSen>().CreateClone();',
    'var c2 = ModelDb.Card<YinSenSen>().CreateCloneForPlayer(ModelDb.Card<MoRanJiangShan>().ToMutable());',
    'var c3 = canonical.CreateCloneForPlayer(player); var t = other.ToMutable();',
    'var c4 = canonical.CreateClone(); var t2 = canonical.ToMutable();',
    # ★ 第八轮新增：这两个原来被当成「正确写法」放行，实际是 NRE 元凶
    'var card5 = ModelDb.Card<MoRanJiangShan>().ToMutable().CreateCloneForPlayer(player);',
    'var token5 = ModelDb.Card<YinSenSen>().ToMutable().CreateCloneForPlayer(player);',
    'var created5 = canonical.ToMutable().CreateCloneForPlayer(player);',
    'var proto5 = canonical.ToMutable().CreateClone();',
    'var dupe5 = canonical.ToMutable().CreateDupe(player);',
    # ★ 第十轮新增：战斗外只 ToMutable+设Owner 就往 Deck 加 —— 缺 RunState 登记
    #   （文件级判据：本「文件」里没有 RunState.CreateCard/AddCard 就报）
    'await CardPileCmd.Add(card, PileType.Deck, CardPilePosition.Bottom, null, false);',
]
SELF_TEST_CLEAN = [
    'var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1);',
    'await PowerCmd.Apply<JinJianPower>(choiceContext, owner, 1m, applier, cardSource, false);',
    'var v = attacks[Random.Shared.Next(attacks.Count)];',
    'var power = ModelDb.Power<StrengthPower>();',
    # ★ 修好之后的正确写法（ToMutable → 设 Owner → 交给框架）不应命中
    'var card = ModelDb.Card<MoRanJiangShan>().ToMutable();',
    'card.Owner = player;',
    'var token = player.Creature.CombatState.CreateCard(ModelDb.Card<YinSenSen>(), player);',
    'await CardPileCmd.AddGeneratedCardToCombat(created, PileType.Hand, player, CardPilePosition.Random);',
    # ★ 战斗外写 Deck 但**已**登记进 RunState —— 应放行。
    #   注意：这条规则是**文件级**判据（Deck 写入 + 文件内有 RunState 登记），
    #   所以这两个片段必须放进「文件级」用例里成对验证，不能逐行单测。
    'var card7 = runState.CreateCard(ModelDb.Card<MoRanJiangShan>(), player);',
    'await CardPileCmd.Add(card7, PileType.Deck, CardPilePosition.Bottom, null, false);',
]
# 逐行单测时需跳过、只在「文件级」用例里验证的片段（前缀匹配）
SELF_TEST_FILE_ONLY = (
    'var card7 = runState.CreateCard(',
    'await CardPileCmd.Add(card7, PileType.Deck',
)


def self_test(model_classes):
    new_model_rx = re.compile(
        r'\bnew\s+(' + '|'.join(sorted(map(re.escape, model_classes))) + r')\s*\(\s*\)'
    ) if model_classes else None

    def hit_list(text, file_text=None):
        """file_text 用于模拟「文件级」判据：给了它就用它判 RunState 登记。"""
        body = file_text if file_text is not None else text
        hits = [n for n, rx, h in RULES if rx.search(text)]
        hits += [n for n, raw, ok, h in RULES_CHAIN if raw.search(text) and not ok_chain(text, ok)]
        for n, raw, ok, h in RULES_DECK_RUNSTATE:
            if raw.search(text) and not ok.search(body):
                hits.append(n)
        if new_model_rx and new_model_rx.search(text):
            hits.append('new <模型类>() 直接构造')
        return hits

    print('--- 规则自检（应命中）')
    bad = 0
    for t in SELF_TEST_HIT:
        hits = hit_list(t)
        if not hits:
            bad += 1
        print('  %s %s' % ('HIT ' if hits else 'MISS', t[:78]))
    print('--- 规则自检（不应命中）')
    for t in SELF_TEST_CLEAN:
        if t.startswith(SELF_TEST_FILE_ONLY):
            continue   # 文件级判据的片段，交给下面成对验证
        hits = hit_list(t)
        if hits:
            bad += 1
        print('  %s %s -> %s' % ('BAD-HIT' if hits else 'clean ', t[:70], hits))
    print('--- 规则自检（文件级：Deck 写入 + 已登记 RunState，整文件应放行）')
    joined_ok = '\n'.join(SELF_TEST_CLEAN)
    joined_bad = '\n'.join(
        ['var card = ModelDb.Card<MoRanJiangShan>().ToMutable();',
         'card.Owner = player;',
         'await CardPileCmd.Add(card, PileType.Deck, CardPilePosition.Bottom, null, false);']
    )
    h_ok = hit_list(
        'await CardPileCmd.Add(card7, PileType.Deck, CardPilePosition.Bottom, null, false);',
        file_text=joined_ok)
    h_bad = hit_list(
        'await CardPileCmd.Add(card, PileType.Deck, CardPilePosition.Bottom, null, false);',
        file_text=joined_bad)
    if h_ok:
        bad += 1
    if not h_bad:
        bad += 1
    print('  %s 文件含 RunState 登记 -> %s' % ('BAD-HIT' if h_ok else 'clean ', h_ok))
    print('  %s 文件无 RunState 登记 -> %s' % ('HIT ' if h_bad else 'MISS', h_bad))
    print('自检异常项: %d' % bad)
    print()
    return bad


def main():
    model_classes = collect_model_classes()
    findings, scanned = scan(model_classes)
    print('=== 危险模式扫描 ===')
    print('扫描 %d 个 .cs 文件；识别出 %d 个本工程模型类' % (scanned, len(model_classes)))
    if not findings:
        print('全部通过：未发现已知的崩溃/卡死写法。')
    else:
        print('发现 %d 处：' % len(findings))
        for rel, n, name, line, hint in findings:
            print()
            print('  [%s] %s:%d' % (name, rel, n))
            print('      %s' % line[:140])
            print('      -> %s' % hint)

    rc = 0 if not findings else 1
    if '--self-test' in sys.argv:
        print()
        rc |= 1 if self_test(model_classes) else 0
    return rc


if __name__ == '__main__':
    sys.exit(main())
