# -*- coding: utf-8 -*-
r"""从 v0.5 生成器的 doc 字段自动产出卡牌文案（zh + en）。

做法：
  1. 解析规格拿到 卡名 / doc / DynamicVars 基值
  2. 从 doc 里剥掉设计注（★ 行）与「升级后…」句
  3. 把数值按变量值替换成 {Var:diff()} 占位符（同一数值优先匹配未用过的变量）
  4. 英文用术语词典机械翻译（可读、可交付；后续可人工润色）
"""
import io, re, json

GEN = r'C:\Users\wangx\WorkBuddy\_scratch\sts2-mod\gen_cards_v05.py'
OUT = (r'C:\Users\wangx\Documents\Default Project\WanJieRuLin'
       r'\.tools\card_loc_v05.py')

src = io.open(GEN, encoding='utf-8').read()
idx = [m.start() for m in re.finditer(r'^card\(', src, re.M)]

cards = []
for k, i in enumerate(idx):
    j = idx[k + 1] if k + 1 < len(idx) else src.find('# ================', i)
    if j < 0:
        j = len(src)
    b = src[i:j]
    m = re.match(r"card\('(\w+)',\s*'([^']*)',\s*(-?\d+),\s*'(\w+)',\s*'(\w+)',\s*'(\w+)'", b)
    if not m:
        continue
    cls, cn, cost, typ, rar, tgt = m.groups()
    dm = re.search(r"doc='(.*?)'\)", b, re.S)
    doc = dm.group(1) if dm else ''
    doc = doc.replace('\\n', '\n')

    # 变量基值
    vals = {}
    body = ''
    vm = re.search(r'vars_=\[(.*?)\]', b, re.S)
    if vm:
        body = vm.group(1)
        d = re.search(r'DV % (-?\d+)', body)
        if d:
            vals['Damage'] = int(d.group(1))
        d = re.search(r'BV % (-?\d+)', body)
        if d:
            vals['Block'] = int(d.group(1))
        d = re.search(r'vc\((\d+)\)', body)
        if d:
            vals['Cards'] = int(d.group(1))
        d = re.search(r'vr\((\d+)\)', body)
        if d:
            vals['Repeat'] = int(d.group(1))
        for nm, v in re.findall(r"vi\('(\w+)',\s*(-?\d+)\)", body):
            vals.setdefault(nm, int(v))

    # 取「效果正文」：第一段（★ 行之前）
    lines = []
    for ln in doc.split('\n'):
        t = ln.strip()
        if not t:
            continue
        # ★ 行若带「——」，破折号后面才是效果正文；不带则整行是设计注，跳过
        if t.startswith('★'):
            if '——' in t:
                lines.append(t.split('——', 1)[1].strip())
            continue
        lines.append(t)
        if '升级后' in t or '升级 ★' in t:
            break
    eff = ' '.join(lines)
    eff = re.sub(r'升级后.*$', '', eff).strip()
    eff = re.sub(r'升级 ★.*$', '', eff).strip()
    eff = eff.rstrip('。') + '。' if eff else ''
    eff = eff.replace('**', '').replace('[gold]', '').replace('[/gold]', '')
    eff = eff.replace('[blue]', '').replace('[/blue]', '')
    # 去掉拼接留下的引号/空白残渣
    eff = eff.strip().strip("'").strip()
    eff = eff.replace("' '", ' ').replace("''", '')
    eff = eff.strip().strip("'").strip()
    eff = re.sub(r'\s+', ' ', eff).strip()
    eff = re.sub(r'\s*。\s*$', '。', eff)
    eff = eff.replace('  。', '。').replace(' 。', '。')
    eff = re.sub(r'。+', '。', eff)
    # 兜底：若仍为空，用整段 doc 去掉 ★ 标记
    if len(eff) <= 2:
        fb = doc.replace('\n', ' ')
        for t in ('★', '——', '**', '[gold]', '[/gold]', '[blue]', '[/blue]'):
            fb = fb.replace(t, '')
        fb = re.sub(r'升级后.*$', '', fb).strip()
        eff = fb.rstrip('。') + '。' if fb else eff

    cards.append(dict(cls=cls, cn=cn, cost=int(cost), typ=typ, rar=rar,
                      vals=vals, eff=eff, doc=doc))

print('解析卡牌:', len(cards))

# ---------------------------------------------------------------- 数值 → 占位符
ORDER = ['Damage', 'Block', 'Cards', 'Repeat', 'Bonus', 'Edge', 'Force', 'MoYun',
         'ThinBonus', 'EvenBonus', 'QiSwing', 'BloodCost', 'StrengthLoss',
         'Vulnerable', 'Weak', 'NextTurn', 'NextTurnBlock', 'BlockPerQi',
         'PerEdge', 'PerForce', 'PerMoYun', 'PerTwo', 'MoYunCost', 'EmptyBonus',
         'EmptyDraw', 'SetTo', 'GhostQiGain', 'Cap', 'PerStep', 'Amount', 'Turns',
         'BonusDamage', 'BonusPerQi', 'QiPerEnergy', 'QiPerCard', 'FreeCards',
         'BonusStat', 'BonusFree', 'BonusDraw', 'DrawPerAttack', 'MaxSteps',
         'BonusPerStep', 'HighBonus', 'QiPerHit', 'QiPerBonus', 'BonusPer',
         'Base', 'Discard', 'MaxTriggersPerTurn', 'MaxBlockPerTurn',
         'EnergyPerTurn', 'EnergyNextTurn', 'GhostQiNextTurn', 'GhostQiLoss',
         'QiPerExtraHit', 'PerQi', 'PerCard', 'DrawPerTurn', 'AmountPerTurn',
         'Threshold', 'SwordsPerTurn', 'AttacksPerGhostQi', 'QiPerMoYun',
         'PerTwo', 'MoYunPerCard']


def placeholders(text, vals):
    used = set()
    # 按变量声明顺序匹配，避免同一数值被用两次
    items = [(k, vals[k]) for k in ORDER if k in vals and vals[k] > 1]
    for name, v in items:
        s = str(v)
        if s in used:
            continue
        if s in text:
            text = text.replace(s, '{%s:diff()}' % name, 1)
            used.add(s)
    # 剩余的数值变量（=1 的）用「1」
    for name, v in vals.items():
        if v == 1 and name not in ('Damage', 'Block'):
            text = re.sub(r'(?<![\d}])1(?![\d%])', '{%s:diff()}' % name, text, count=1)
    return text


# ---------------------------------------------------------------- 英文机械翻译
DICT = [
    ('对所有敌人造成', 'Deal {X} to ALL enemies'.replace('{X}', '')), ('对全体敌人造成', 'Deal '),
    ('造成', 'Deal '), ('点伤害', ' damage'), ('伤害', 'damage'),
    ('获得', 'Gain '), ('点格挡', ' Block'), ('格挡', 'Block'),
    ('抽', 'Draw '), ('张牌', ' card(s)'), ('点能量', ' Energy'), ('能量', 'Energy'),
    ('点笔锋', ' Edge'), ('笔锋', 'Edge'), ('点力道', ' Force'), ('力道', 'Force'),
    ('点墨韵', ' Ink'), ('墨韵', 'Ink'),
    ('失去', 'Lose '), ('所有', 'all '), ('消耗', 'Spend '), ('每', 'per '),
    ('若', 'If '), ('你', 'you '), ('本回合', 'this turn'), ('回合', 'turn'),
    ('敌人', 'enemy '), ('手牌', 'hand'), ('为空', 'is empty'), ('高于', 'more than'),
    ('不高于', 'or less'), ('不低于', 'or more'), ('最多', 'up to '),
    ('次', ' time(s)'), ('层', ' stack(s)'), ('点力量', ' Strength'),
    ('易伤', 'Vulnerable'), ('虚弱', 'Weak'), ('生命', 'HP'), ('受到', 'Take '),
    ('升级后', ''), ('。', '. '), ('，', ', '), ('、', ', '),
]


def to_en(zh):
    t = zh
    for a, b in DICT:
        t = t.replace(a, b)
    t = re.sub(r'\s+', ' ', t).strip()
    t = t.replace('{Damage:diff()} damage damage', '{Damage:diff()} damage')
    return t


# ---------------------------------------------------------------- 写文件
ZH, EN, TZ, TE = {}, {}, {}, {}
for c in cards:
    zh = placeholders(c['eff'], c['vals'])
    ZH[c['cls']] = zh
    EN[c['cls']] = to_en(zh)
    TZ[c['cls']] = c['cn']
    # 英文名：用中文名（后续可人工润色，但不留空）
    TE[c['cls']] = c['cn']

out = ['# -*- coding: utf-8 -*-',
       'r"""v0.5 卡牌文案（自动生成，可用 gen_cards_v05.py 的 doc 重跑同步）。"""',
       '', 'ZH = {']
for c in cards:
    out.append("    '%s': '%s'," % (c['cls'], ZH[c['cls']].replace("'", "\\'")))
out.append('}')
out.append('')
out.append('EN = {')
for c in cards:
    out.append("    '%s': '%s'," % (c['cls'], EN[c['cls']].replace("'", "\\'")))
out.append('}')
out.append('')
out.append('TITLES_ZH = {')
for c in cards:
    out.append("    '%s': '%s'," % (c['cls'], c['cn']))
out.append('}')
out.append('')
out.append('TITLES_EN = {')
for c in cards:
    out.append("    '%s': '%s'," % (c['cls'], TE[c['cls']]))
out.append('}')
io.open(OUT, 'w', encoding='utf-8', newline='\n').write('\n'.join(out) + '\n')
print('写出', OUT)
print()
for c in cards[:6]:
    print('  %-16s %s' % (c['cn'], ZH[c['cls']][:70]))
