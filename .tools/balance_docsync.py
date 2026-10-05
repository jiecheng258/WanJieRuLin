# -*- coding: utf-8 -*-
r"""同步 doc 文案里的数字（数值上调后，文案里的旧数字会导致占位符插入失败）。

做法：对每张卡，遍历它的 var 值 V（新值），反推旧值候选（V/1.2 的 floor/round/ceil），
把 doc 里第一次出现的旧值换成……**不换**！
—— 因为文案里应该是 {Var:diff()} 占位符，只要让匹配函数能「按新值找到位置」即可。

所以这里改的是：把 doc 里的旧数字直接替换成新数字，
这样 _v05_loc.py 的 placeholders() 就能按新值匹配到，插入占位符 ✓
"""
import io, re, math

P = r'C:\Users\wangx\WorkBuddy\_scratch\sts2-mod\gen_cards_v05.py'
s = io.open(P, encoding='utf-8').read()
idx = [m.start() for m in re.finditer(r'^card\(', s, re.M)]
FACTOR = 1.2


def candidates(new_v):
    """反推旧值候选（含 up() 的 ceil/round 两种取整）。"""
    out = set()
    for r in (new_v / FACTOR, (new_v - 1) / FACTOR, (new_v + 0.5) / FACTOR):
        for f in (math.floor, round, math.ceil):
            v = int(f(r))
            if v > 0 and v != new_v:
                out.add(v)
    return sorted(out, reverse=True)


changed = 0
cards_touched = 0
# 从后往前处理，避免位移
for k in range(len(idx) - 1, -1, -1):
    i = idx[k]
    j = idx[k + 1] if k + 1 < len(idx) else len(s)
    block = s[i:j]

    # 取该卡的 var 值（新值）
    news = []
    m = re.search(r'vars_=\[(.*?)\]', block, re.S)
    if m:
        body = m.group(1)
        for pat in (r'DV % (\d+)', r'BV % (\d+)', r'vc\((\d+)\)'):
            news += [int(x) for x in re.findall(pat, body)]

    if not news:
        continue

    # 取 doc
    dm = re.search(r"doc='(.*?)'\)", block, re.S)
    if not dm:
        continue
    doc = dm.group(1)
    new_doc = doc
    n_here = 0
    for v in news:
        for old in candidates(v):
            if str(old) in new_doc:
                new_doc = new_doc.replace(str(old), str(v), 1)
                n_here += 1
                break

    if new_doc != doc:
        s = s[:i] + block.replace(doc, new_doc, 1) + s[j:]
        changed += n_here
        cards_touched += 1

io.open(P, 'w', encoding='utf-8', newline='\n').write(s)
import ast
ast.parse(io.open(P, encoding='utf-8').read())
print('同步了 %d 张卡、%d 处数字' % (cards_touched, changed))
print()
print('=== 抽样 ===')
for cls in ('DaJi', 'HengXian', 'ChengFu', 'PoMian', 'DaPoMo'):
    a = s.find("card('%s'" % cls)
    seg = s[a:a + 700]
    d = re.search(r"doc='(.*?)'\)", seg, re.S)
    v = re.search(r'vars_=\[(.*?)\]', seg, re.S)
    print('  %-14s vars=%-22s doc=%s' % (
        cls, (v.group(1).strip() if v else '?')[:20],
        (d.group(1).split('\\n')[0][:56] if d else '?')))
