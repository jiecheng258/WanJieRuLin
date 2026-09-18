# -*- coding: utf-8 -*-
"""占位符审计。

检查三类文案里的 {Var} 是否都能在对应的变量集合里找到：

1. 卡牌：cards.json 的 description ↔ 该卡 CanonicalVars
2. 能力：powers.json 的 description/smartDescription ↔ 该能力 CanonicalVars（+ Amount）
3. 先古事件选项：选项文案**不允许**出现任何 {占位符}

关于命名规则（这是本脚本最容易写错的地方，已经踩过一次）：
  ModCardVars.Power<TPower>(n) 的变量名 == typeof(TPower).Name，**带结尾的 "Power"**！
  它内部就是 Type.GetTypeFromHandle(...).Name，不做任何裁剪。
  所以 Power<HeiAnBiZhangTempDexterityPower> 生成的变量名是
  HeiAnBiZhangTempDexterityPower，而不是 HeiAnBiZhangTempDexterity。
  之前这里把「带 Power」和「去掉 Power」两种名字都塞进白名单，
  结果真出问题时审计一声不吭（游戏里却报
  "No source extension could handle the selector named ..."）。
"""
import io, json, os, re

ROOT = r'C:\Users\wangx\Documents\Default Project\WanJieRuLin\WanJieRuLin'
CODE = os.path.join(r'C:\Users\wangx\Documents\Default Project\WanJieRuLin', 'WanJieRuLinCode')
CARD_DIR = os.path.join(CODE, 'Cards')
POWER_DIR = os.path.join(CODE, 'Powers')

# 不是「具体卡牌/能力」的文件：基类、工具类。它们不该被当成具体内容去查本地化键。
NON_CARD_FILES = {'WanJieRuLinCardModel', 'Swords'}
NON_POWER_FILES = {'WanJieTempAppliedPower', 'WanJieTempAppliedPowers', 'WanJieTempAppliedPowers'}

# 框架/原版注入的通用变量，文案里可以直接用，不算缺失。
GLOBAL_VARS = {
    'Amount', 'IfUpgraded', 'OnTable', 'InCombat', 'IsTargeting', 'TargetType',
    'GainsBlock', 'IsOstyAlive', 'PlayerCount', 'IsMultiplayer', 'OnPlayer',
    'OwnerName', 'ApplierName', 'character', 'characterObject', 'characterGender',
    'possessiveAdjective', 'pronounObject', 'pronounPossessive', 'pronounSubject',
}

# 先古事件选项文案的键根：这些键下的文案不许出现占位符。
ANCIENT_OPTION_PREFIX = 'WAN_JIE_RU_LIN_CARD_MO_RAN_JIANG_SHAN_OPTION'

_CONST_CACHE = {}


def resolve_const(src, ident):
    """把 `private const string X = "Y";` 解析成 Y；解析不到就返回 ident 本身。"""
    if ident == 'GhostQiCostVarName':
        return 'GhostQiCost'
    key = (id(src), ident)
    if key in _CONST_CACHE:
        return _CONST_CACHE[key]
    m = re.search(r'const\s+string\s+' + re.escape(ident) + r'\s*=\s*"([^"]+)"', src)
    out = m.group(1) if m else ident
    _CONST_CACHE[key] = out
    return out


def vars_of(src):
    """从源码的 CanonicalVars 里抓变量名。"""
    names = set()
    m = re.search(r'CanonicalVars\s*=>\s*\[(.*?)\];', src, re.S)
    if not m:
        m = re.search(r'CanonicalVars\s*=>\s*(.+?);', src, re.S)
    body = m.group(1) if m else ''

    # var XxxVar(...) / new XxxVar(...) → 变量名 = Var 前面的词（BlockVar → Block）
    for mm in re.finditer(r'new\s+(\w+)Var\s*\(', body):
        names.add(mm.group(1))

    # ModCardVars.Int("Name", n) / 其它显式名字的工厂
    for mm in re.finditer(r'ModCardVars\.\w+\(\s*"([^"]+)"', body):
        names.add(mm.group(1))
    # 名字写成 const string 的情况
    for mm in re.finditer(r'ModCardVars\.\w+\(\s*([A-Za-z_]\w*)\s*,', body):
        names.add(resolve_const(src, mm.group(1)))

    # 基类提供的鬼气费用/获得变量
    if re.search(r'GhostQiCostVarOf\s*\(', body):
        names.add('GhostQiCost')
    if re.search(r'GhostQiGainVarOf\s*\(', body):
        names.add('GhostQiGain')

    # ★ ModCardVars.Power<TPower>(n)：变量名就是完整类型名（带 Power）。
    for mm in re.finditer(r'ModCardVars\.Power<(\w+)>', body):
        names.add(mm.group(1))
    # ComputedPower<T> / ComputedPowerAmountGiven<T> 同理。
    for mm in re.finditer(r'ModCardVars\.Computed\w*<(\w+)>', body):
        names.add(mm.group(1))

    # 无参工厂：名字是固定的
    if re.search(r'ModCardVars\.Cards\(\s*[^"\s]', body):
        names.add('Cards')
    for fixed, name in (('Energy', 'Energy'), ('Repeat', 'Repeat'), ('Gold', 'Gold'),
                        ('Heal', 'Heal'), ('Damage', 'Damage'), ('Block', 'Block'),
                        ('Forge', 'Forge'), ('Summon', 'Summon'), ('Stars', 'Stars'),
                        ('HpLoss', 'HpLoss'), ('MaxHp', 'MaxHp')):
        if re.search(r'ModCardVars\.' + fixed + r'\(\s*[^"\s]', body):
            names.add(name)
    return names


def upper_snake(name):
    stem, tail = name, ''
    if name.endswith('Power') and len(name) > len('Power'):
        stem, tail = name[:-len('Power')], '_POWER'
    out = []
    for i, ch in enumerate(stem):
        if ch.isupper() and i > 0:
            prev = stem[i - 1]
            nxt = stem[i + 1] if i + 1 < len(stem) else ''
            if prev.islower() or (prev.isupper() and nxt.islower()):
                out.append('_')
        out.append(ch.upper())
    return ''.join(out) + tail


def placeholders(text):
    """取出文案里的 {Var} / {Var:selector()} 中的 Var。"""
    return [m.group(1) for m in re.finditer(r'\{(\w+)(?::[a-zA-Z_]\w*(?:\([^)]*\))?)?\}', text)]


problems = []

# ---------------------------------------------------------------- 1) 卡牌
loc_zh_cards = json.load(io.open(os.path.join(ROOT, 'localization', 'zhs', 'cards.json'), encoding='utf-8'))
loc_en_cards = json.load(io.open(os.path.join(ROOT, 'localization', 'eng', 'cards.json'), encoding='utf-8'))

for fn in sorted(os.listdir(CARD_DIR)):
    if not fn.endswith('.cs'):
        continue
    cls = fn[:-3]
    if cls in NON_CARD_FILES:
        continue
    src = io.open(os.path.join(CARD_DIR, fn), encoding='utf-8').read()
    if 'CanonicalVars' not in src:
        continue
    if re.search(r'abstract\s+class\s+' + re.escape(cls) + r'\b', src):
        continue
    key = 'WAN_JIE_RU_LIN_CARD_' + upper_snake(cls)
    have = vars_of(src) | GLOBAL_VARS
    for lang, loc in (('zhs', loc_zh_cards), ('eng', loc_en_cards)):
        desc = loc.get(key + '.description')
        if desc is None:
            problems.append((cls, lang, 'NO_LOC_KEY', ''))
            continue
        for v in placeholders(desc):
            if v not in have:
                problems.append((cls, lang, 'MISSING_VAR', v + '  (have: ' + ','.join(sorted(have)) + ')'))

# ---------------------------------------------------------------- 2) 能力
loc_zh_powers = json.load(io.open(os.path.join(ROOT, 'localization', 'zhs', 'powers.json'), encoding='utf-8'))
loc_en_powers = json.load(io.open(os.path.join(ROOT, 'localization', 'eng', 'powers.json'), encoding='utf-8'))

for fn in sorted(os.listdir(POWER_DIR)):
    if not fn.endswith('.cs'):
        continue
    cls = fn[:-3]
    if cls in NON_POWER_FILES:
        continue
    src = io.open(os.path.join(POWER_DIR, fn), encoding='utf-8').read()
    if '[RegisterPower' not in src:
        continue
    if re.search(r'abstract\s+class\s+' + re.escape(cls) + r'\b', src):
        continue
    key = 'WAN_JIE_RU_LIN_POWER_' + upper_snake(cls)
    have = vars_of(src) | GLOBAL_VARS
    for lang, loc in (('zhs', loc_zh_powers), ('eng', loc_en_powers)):
        for suffix in ('.description', '.smartDescription'):
            text = loc.get(key + suffix)
            if text is None:
                problems.append((cls, lang, 'NO_LOC_KEY', key + suffix))
                continue
            for v in placeholders(text):
                if v not in have:
                    problems.append((cls, lang, 'MISSING_VAR', suffix + ' ' + v + '  (have: ' + ','.join(sorted(have)) + ')'))

print('=== 占位符审计（卡牌 + 能力）===')
if not problems:
    print('全部通过。')
for cls, lang, kind, extra in problems:
    print('%-24s %-4s %-14s %s' % (cls, lang, kind, extra))

# ---------------------------------------------------------------- 3) 先古选项文案
print()
print('=== 先古事件选项文案（不允许出现占位符）===')
bad_opt = []
for lang in ('zhs', 'eng'):
    loc = json.load(io.open(os.path.join(ROOT, 'localization', lang, 'cards.json'), encoding='utf-8'))
    found = {k: v for k, v in loc.items() if k.startswith(ANCIENT_OPTION_PREFIX)}
    if not found:
        bad_opt.append((lang, 'NO_KEY', ANCIENT_OPTION_PREFIX + '.description 缺失'))
    for k, v in found.items():
        for ph in placeholders(v):
            bad_opt.append((lang, k, '含占位符 {' + ph + '} —— 事件界面解析不到卡牌变量，会刷日志'))
if not bad_opt:
    print('无问题。')
for lang, k, why in bad_opt:
    print('%-4s %-58s %s' % (lang, k, why))

# ---------------------------------------------------------------- 4) 残留「升级后」
print()
print('=== 残留“升级后/Upgraded”检查 ===')
bad = []
for lang in ('zhs', 'eng'):
    for table in ('cards', 'relics', 'powers', 'characters', 'ancients'):
        p = os.path.join(ROOT, 'localization', lang, table + '.json')
        if not os.path.exists(p):
            continue
        obj = json.load(io.open(p, encoding='utf-8'))
        for k, v in obj.items():
            if isinstance(v, str) and ('升级后' in v or 'Upgraded' in v):
                bad.append((lang, table, k, v[:80]))
if not bad:
    print('无残留。')
for b in bad:
    print('%-4s %-12s %-52s %s' % b)
print()
print('files checked OK')
