# -*- coding: utf-8 -*-
"""审计：每张牌的「鬼气费用」在代码与本地化文案之间是否一致。

判定规则（精确版）：
  * 代码里挂了鬼气费用 → 卡面描述<b>第一行</b>必须出现费用行（中文「耗费」/ 英文 "Spend"）。
  * 代码里没挂费用 → 全文不得出现费用行用词（否则玩家会以为要付费）。

注意不要把「获得 N 点鬼气」这类<b>效果</b>误判成费用 —— 早期版本用
「文案里出现数字+鬼气」做启发式，会把增气牌全部误报，已废弃。
"""
import io, json, os, re, sys

ROOT = r'C:\Users\wangx\Documents\Default Project\WanJieRuLin\WanJieRuLin'
CODE = r'C:\Users\wangx\Documents\Default Project\WanJieRuLin\WanJieRuLinCode'

COST_VERB_ZH = '耗费'
COST_VERB_EN = 'Spend'


def upper_snake(name):
    """类名 → 本地化键用的 UPPER_SNAKE（trailing Power 不拆）。"""
    stem, tail = name, ''
    if name.endswith('Power') and len(name) > len('Power'):
        stem, tail = name[:-len('Power')], '_POWER'
    s = re.sub(r'(?<=[a-z0-9])([A-Z])', r'_\1', stem)
    s = re.sub(r'(?<=[A-Z])([A-Z][a-z])', r'_\1', s)
    return (s + tail).upper()


def parse_costs():
    """扫描代码，返回 {类名: 'X' 或 int}。"""
    costs = {}
    for dp, dn, fn in os.walk(CODE):
        for f in sorted(fn):
            if not f.endswith('.cs'):
                continue
            src = io.open(os.path.join(dp, f), encoding='utf-8', errors='replace').read()
            for m in re.finditer(r'class\s+(\w+)\s*:\s*WanJieRuLinCardModel', src):
                cls = m.group(1)
                start = m.end()
                nxt = src.find('class ', start)
                seg = src[start:nxt if nxt != -1 else len(src)]
                if 'SetGhostQiCostX' in seg:
                    costs[cls] = 'X'
                    continue
                mm = re.search(r'SetGhostQiCost\s*\(\s*([A-Za-z_0-9]+)\s*\)', seg)
                if not mm:
                    continue
                arg = mm.group(1)
                if arg.isdigit():
                    costs[cls] = int(arg)
                else:
                    c = re.search(re.escape(arg) + r'\s*=\s*(\d+)', src)
                    if c:
                        costs[cls] = int(c.group(1))
    return costs


def main():
    costs = parse_costs()
    zh = json.load(io.open(os.path.join(ROOT, 'localization', 'zhs', 'cards.json'), encoding='utf-8'))
    en = json.load(io.open(os.path.join(ROOT, 'localization', 'eng', 'cards.json'), encoding='utf-8'))

    by_snake = {upper_snake(c): c for c in costs}

    problems, stray = [], []
    print('%-22s %-9s %-8s %s' % ('类名', '代码费用', '首行声明', '状态'))
    print('-' * 78)

    for cls in sorted(costs):
        c = costs[cls]
        key = 'WAN_JIE_RU_LIN_CARD_' + upper_snake(cls) + '.description'
        tz, te = zh.get(key, ''), en.get(key, '')
        if not tz or not te:
            problems.append((cls, '缺少本地化条目 ' + key))
            print('%-22s %-9s %-8s %s' % (cls, c, '-', '!! 缺本地化'))
            continue
        ok = (COST_VERB_ZH in tz.split('\n')[0]) and (COST_VERB_EN in te.split('\n')[0])
        print('%-22s %-9s %-8s %s' % (
            cls, 'X' if c == 'X' else c, '是' if ok else '否',
            'OK' if ok else '!! 首行未声明费用'))
        if not ok:
            problems.append((cls, '首行未声明鬼气费用'))

    # 反向：代码没收费却出现费用行
    for k, v in zh.items():
        if not k.endswith('.description'):
            continue
        m = re.match(r'WAN_JIE_RU_LIN_CARD_(.+)\.description$', k)
        if not m:
            continue
        snake = m.group(1)
        cls = by_snake.get(snake)
        head = v.split('\n')[0]
        if COST_VERB_ZH in head and (cls is None):
            stray.append((k, head))

    print()
    print('收费卡总数: %d' % len(costs))
    print()
    print('=== 问题汇总 ===')
    if not problems and not stray:
        print('无问题。')
    for p in problems:
        print(' - %s | %s' % p)
    for k, head in stray:
        print(' - %s | 代码未收费但文案写了费用行: %s' % (k, head))

    return 1 if (problems or stray) else 0


if __name__ == '__main__':
    sys.exit(main())
