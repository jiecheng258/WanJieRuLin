# -*- coding: utf-8 -*-
"""一致性审计：C# 类 ↔ 本地化键 ↔ 美术资源 ↔ 卡池注册"""
import os, re, json, io

PROJ = r'C:\Users\wangx\Documents\Default Project\WanJieRuLin'
CODE = os.path.join(PROJ, 'WanJieRuLinCode')
RES = os.path.join(PROJ, 'WanJieRuLin')
LOC = os.path.join(RES, 'localization')


def upper_snake(name):
    out = []
    for i, ch in enumerate(name):
        if ch.isupper() and i > 0:
            prev = name[i - 1]
            nxt = name[i + 1] if i + 1 < len(name) else ''
            if prev.islower() or (prev.isupper() and nxt.islower()):
                out.append('_')
        out.append(ch.upper())
    return ''.join(out)


def strip_comments(src):
    """去掉 // 行注释与 /* */ 块注释。

    必须做这一步：注释里出现 `[RegisterPower(Inherit = true)]` 这类说明文字时，
    直接把源码当字符串搜属性名会误判（踩过）。
    """
    src = re.sub(r'/\*.*?\*/', '', src, flags=re.S)
    src = re.sub(r'//[^\n]*', '', src)
    return src


def scan_dir(sub):
    """扫描一个子目录下的 .cs，返回 {类名: 信息}。"""
    out = {}
    d = os.path.join(CODE, sub)
    if not os.path.isdir(d):
        return out
    for f in sorted(os.listdir(d)):
        if not f.endswith('.cs'):
            continue
        raw = io.open(os.path.join(d, f), encoding='utf-8').read()
        src = strip_comments(raw)
        for m in re.finditer(r'(?:sealed\s+|abstract\s+)?class\s+(\w+)', src):
            cls = m.group(1)
            out[cls] = dict(
                file=f,
                isCard='[RegisterCard(' in src,
                isRelic='[RegisterRelic(' in src,
                isPower='[RegisterPower' in src,
                rarity=(re.search(r'CardRarity\.(\w+)', src) or [None, None])[1]
                       if re.search(r'CardRarity\.(\w+)', src) else None,
                abstract=re.search(r'abstract\s+class\s+' + re.escape(cls) + r'\b', src) is not None,
            )
    return out


# 1) 扫描代码里注册的类
cards = scan_dir('Cards')
relics = scan_dir('Relics')
powers = scan_dir('Powers')
chars = scan_dir('Characters')

# 2) 读本地化
def load(p):
    if not os.path.exists(p):
        return {}
    return json.load(io.open(p, encoding='utf-8-sig'))

zh_cards = load(os.path.join(LOC, 'zhs', 'cards.json'))
en_cards = load(os.path.join(LOC, 'eng', 'cards.json'))
zh_relics = load(os.path.join(LOC, 'zhs', 'relics.json'))
zh_powers = load(os.path.join(LOC, 'zhs', 'powers.json'))
zh_chars = load(os.path.join(LOC, 'zhs', 'characters.json'))
zh_anc = load(os.path.join(LOC, 'zhs', 'ancients.json'))

print('=' * 72)
print('1. 卡牌：注册 ↔ 本地化 ↔ 卡图')
print('=' * 72)
reg_cards = {c: v for c, v in cards.items() if v['isCard']}
missing_loc, missing_art_ok = [], []
for cls in sorted(reg_cards):
    key = 'WAN_JIE_RU_LIN_CARD_' + upper_snake(cls)
    has_zh = key + '.title' in zh_cards
    has_en = key + '.title' in en_cards
    art = os.path.exists(os.path.join(RES, 'images', 'cards', cls + '.png'))
    flag = 'OK ' if (has_zh and has_en and art) else '!! '
    if not (has_zh and has_en):
        missing_loc.append(cls)
    if not art:
        missing_art_ok.append(cls)
    print('  %s%-24s zh=%-5s en=%-5s art=%-5s %s' %
          (flag, cls, has_zh, has_en, art, reg_cards[cls]['rarity'] or ''))
print('  -> 卡牌总数 %d，缺本地化 %d，缺卡图 %d' % (len(reg_cards), len(missing_loc), len(missing_art_ok)))

print()
print('=' * 72)
print('2. 遗物')
print('=' * 72)
reg_relics = {c: v for c, v in relics.items() if v['isRelic']}
for cls in sorted(reg_relics):
    key = 'WAN_JIE_RU_LIN_RELIC_' + upper_snake(cls)
    has_zh = key + '.title' in zh_relics
    art = os.path.exists(os.path.join(RES, 'images', 'relics', cls + '.png'))
    print('  %s%-24s loc=%-5s art=%s' % ('OK ' if (has_zh and art) else '!! ', cls, has_zh, art))
print('  -> 遗物总数 %d' % len(reg_relics))

print()
print('=' * 72)
print('3. 能力')
print('=' * 72)
reg_powers = {c: v for c, v in powers.items() if v['isPower'] and not v['abstract']}
miss_p = []
for cls in sorted(reg_powers):
    key = 'WAN_JIE_RU_LIN_POWER_' + upper_snake(cls)
    has_zh = key + '.title' in zh_powers
    art = os.path.exists(os.path.join(RES, 'images', 'powers', cls + '.png'))
    if not has_zh or not art:
        miss_p.append((cls, has_zh, art))
    print('  %s%-28s loc=%-5s art=%s' % ('OK ' if (has_zh and art) else '!! ', cls, has_zh, art))
abstract_p = sorted(c for c, v in powers.items() if v['isPower'] and v['abstract'])
if abstract_p:
    print('  （抽象基类，由 [RegisterPower(Inherit = true)] 自动注册具体子类，不单独检查：%s）'
          % ', '.join(abstract_p))
print('  -> 能力总数 %d，有问题 %d' % (len(reg_powers), len(miss_p)))

# 临时能力包装：多个来源共用一张图 + 一条文本，键名由代码写死
# （WanJieTempAppliedPower.LocStem），所以单独校验。
print()
print('  --- 临时能力包装的共享资源 ---')
TEMP_SHARED = {
    'WAN_JIE_RU_LIN_POWER_TEMP_STRENGTH': 'WanJieTempStrengthPower',
    'WAN_JIE_RU_LIN_POWER_TEMP_DEXTERITY': 'WanJieTempDexterityPower',
}
miss_t = []
for key, stem in sorted(TEMP_SHARED.items()):
    has_zh = key + '.title' in zh_powers and key + '.description' in zh_powers
    art = os.path.exists(os.path.join(RES, 'images', 'powers', stem + '.png'))
    if not has_zh or not art:
        miss_t.append(key)
    print('  %s%-40s loc=%-5s art=%s (%s.png)'
          % ('OK ' if (has_zh and art) else '!! ', key, has_zh, art, stem))
if miss_t:
    miss_p.extend((k, 'shared', False) for k in miss_t)

print()
print('=' * 72)
print('4. 角色 / 先古对话')
print('=' * 72)
C = 'WAN_JIE_RU_LIN_CHARACTER_WAN_JIE_RU_LIN_CHARACTER'
for suffix in ('title', 'description', 'selectMessage', 'flavor', 'defeatMessage',
               'victoryMessage', 'pronounSubject', 'goldMonologue'):
    k = C + '.' + suffix
    print('  %s %-24s %s' % ('OK ' if k in zh_chars else '!! ', suffix, zh_chars.get(k, '')[:44]))
print('  先古对话条目: %d' % len(zh_anc))
for k, v in sorted(zh_anc.items()):
    print('     %s' % k)

print()
print('=' * 72)
print('5. 卡池注册完整性（skill 卡是否都进了 PickRandomSkillType）')
print('=' * 72)
pool = io.open(os.path.join(CODE, 'Characters', 'WanJieRuLinCardPool.cs'), encoding='utf-8').read()
m = re.search(r'SkillCardTypes\s*=\s*\[(.*?)\];', pool, re.S)
body = m.group(1) if m else ''
listed = set(re.findall(r'typeof\((?:Cards\.)?(\w+)\)', body))
skill_classes = set()
for cls, v in reg_cards.items():
    src = io.open(os.path.join(CODE, 'Cards', v['file']), encoding='utf-8').read()
    if 'CardType.Skill' in src and 'CardRarity.Basic' not in src:
        skill_classes.add(cls)
missing_in_pool = sorted(skill_classes - listed)
extra_in_pool = sorted(listed - skill_classes)
print('  可入池 Skill 卡 %d 张，卡池列表 %d 项' % (len(skill_classes), len(listed)))
print('  未列入卡池的 Skill: %s' % (missing_in_pool or '无'))
print('  卡池里多出的（应为先古/事件专属）: %s' % (extra_in_pool or '无'))

print()
print('=' * 72)
print('6. 先古之民（原版机制复用）接线')
print('=' * 72)
anc_file = os.path.join(CODE, 'Ancients', 'WanJieRuLinAncientOptions.cs')
if os.path.exists(anc_file):
    anc = io.open(anc_file, encoding='utf-8').read()
    # 检查注入的原版先古
    registered = re.findall(r'ModAncientOptionRegistry\.Register<(\w+)>', anc)
    print('  注入的原版先古: %s' % (registered or '无'))
    # 对话覆盖检查：注入的先古必须有 talk 本地化
    for name in registered:
        key = '%s.talk.' % name.upper()
        has = any(k.startswith(key) for k in zh_anc)
        print('    %s 对话本地化: %s' % (name, '有' if has else '!! 缺失'))
    # 选项文案来源
    if 'TitleLocString' in anc:
        print('  选项文案: 复用卡牌 TitleLocString（推荐）')
    elif 'ancientOption.title' in anc:
        print('  选项文案: 独立 ancientOption.title 键')
    else:
        print('  !! 选项文案来源不明确')
    # Entry 是否调用 Register()
    entry = io.open(os.path.join(CODE, 'Entry.cs'), encoding='utf-8').read()
    print('  Entry 已调用 Register(): %s' % ('是' if 'WanJieRuLinAncientOptions.Register()' in entry else '!! 否'))
else:
    print('  !! 未找到 Ancients/WanJieRuLinAncientOptions.cs')

# 古旧尖牙超越映射（绘 -> 墨染江山）
hui = io.open(os.path.join(CODE, 'Cards', 'Hui.cs'), encoding='utf-8').read()
mm = re.search(r'RegisterArchaicToothTranscendence\(typeof\((\w+)\)\)', hui)
print('  古旧尖牙超越映射: %s' % ('Hui -> %s' % mm.group(1) if mm else '!! 未配置'))

print()
print('=' * 72)
print('7. 未注册为卡/遗物/能力的类')
print('=' * 72)
for cls, v in sorted(cards.items()):
    if not v['isCard']:
        print('  Cards/%s  <- %s' % (cls, '基类' if v['abstract'] else '工具类'))
for cls, v in sorted(relics.items()):
    if not v['isRelic']:
        print('  Relics/%s  <- %s' % (cls, '基类' if v['abstract'] else '工具类'))
print()
print('审计完成')
