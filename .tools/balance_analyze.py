# -*- coding: utf-8 -*-
"""把 carddata.txt 解析成结构化数据，统计原版基线。"""
import io, re, json, os, collections

P = r'C:\Users\wangx\WorkBuddy\2026-09-15-20-17-56\.tools\carddata.txt'
t = io.open(P, encoding='utf-8', errors='replace').read()

def parse(section):
    i = t.find(section)
    if i < 0: return []
    j = t.find('#### ', i + 10)
    body = t[i:j if j > 0 else len(t)]
    out = []
    for line in body.split('\n'):
        if '\t' not in line: continue
        name, rest = line.split('\t', 1)
        name = name.strip()
        if not name or name.startswith('#'): continue
        rec = {'name': name, 'raw': rest}
        m = re.match(r'cost=(\S+)\s*\|\s*(\w+)\s*\|\s*(\w+)\s*\|\s*(\w+)', rest)
        if not m:
            rec['fail'] = rest
            out.append(rec); continue
        rec['cost'] = m.group(1); rec['type'] = m.group(2)
        rec['rarity'] = m.group(3); rec['target'] = m.group(4)
        vm = re.search(r'\|\s*vars=([^|]*)\|\s*UP', rest)
        um = re.search(r'\|\s*UP\s*(?:cost=(\S+)\s*)?vars=(.*)$', rest)
        def vlist(s):
            d = {}
            for item in (s or '').split(','):
                item = item.strip()
                if not item or '=' not in item: continue
                k, v = item.split('=', 1)
                k = k.split(':')[-1]
                d[k] = v
            return d
        rec['vars'] = vlist(vm.group(1)) if vm else {}
        if um and 'FAIL' not in (um.group(0) or ''):
            rec['up_cost'] = um.group(1)
            rec['up_vars'] = vlist(um.group(2))
        else:
            rec['up_vars'] = {}
        out.append(rec)
    return out

vanilla = parse('#### 原版卡牌 ####')
mod = parse('#### 万界如林 卡牌 ####')
modpow = parse('#### 万界如林 能力 ####')

print('原版卡牌', len(vanilla), '（构造失败', len([c for c in vanilla if 'fail' in c]), '）')
print('模组卡牌', len(mod), '（构造失败', len([c for c in mod if 'fail' in c]), '）')
print()

def num(d, k):
    v = d.get(k)
    if v is None: return None
    try: return float(v)
    except: return None

# ---- 原版基线：按 费用×类型 统计 Damage / Block ----
print('=' * 78)
print('原版基线：攻击牌 伤害（按费用）')
print('=' * 78)
dmg_by_cost = collections.defaultdict(list)
blk_by_cost = collections.defaultdict(list)
for c in vanilla:
    if 'fail' in c or 'cost' not in c: continue
    if c['type'] == 'Attack':
        v = num(c['vars'], 'Damage')
        if v is not None and c['target'] == 'AnyEnemy':
            dmg_by_cost[c['cost']].append((v, c['name']))
    if c['type'] == 'Skill':
        v = num(c['vars'], 'Block')
        # 纯格挡（只含 Block 一个变量）才算基线，避免把复合效果算进去
        if v is not None and len(c['vars']) == 1:
            blk_by_cost[c['cost']].append((v, c['name']))

for cost in sorted(dmg_by_cost, key=lambda x: (len(x), x)):
    vals = sorted(v for v, _ in dmg_by_cost[cost])
    if len(vals) < 3: continue
    med = vals[len(vals) // 2]
    print('  cost=%-3s n=%-3d 中位=%-6s 范围=%s~%s   例: %s' % (
        cost, len(vals), med, vals[0], vals[-1],
        ', '.join(f'{n}({v:g})' for v, n in dmg_by_cost[cost][:6])))

print()
print('=' * 78)
print('原版基线：纯格挡技能牌 格挡值（按费用）')
print('=' * 78)
for cost in sorted(blk_by_cost, key=lambda x: (len(x), x)):
    vals = sorted(v for v, _ in blk_by_cost[cost])
    if len(vals) < 3: continue
    med = vals[len(vals) // 2]
    print('  cost=%-3s n=%-3d 中位=%-6s 范围=%s~%s   例: %s' % (
        cost, len(vals), med, vals[0], vals[-1],
        ', '.join(f'{n}({v:g})' for v, n in blk_by_cost[cost][:6])))

# ---- 原版升级增量 ----
print()
print('=' * 78)
print('原版升级增量（伤害 / 格挡）')
print('=' * 78)
deltas = collections.Counter(); bdeltas = collections.Counter(); costdn = 0
for c in vanilla:
    if 'fail' in c or not c.get('up_vars'): continue
    d0, d1 = num(c['vars'], 'Damage'), num(c['up_vars'], 'Damage')
    b0, b1 = num(c['vars'], 'Block'), num(c['up_vars'], 'Block')
    if d0 is not None and d1 is not None: deltas[d1 - d0] += 1
    if b0 is not None and b1 is not None: bdeltas[b1 - b0] += 1
    if c.get('up_cost') is not None and c['cost'].isdigit() and c['up_cost'].isdigit() and int(c['up_cost']) < int(c['cost']):
        costdn += 1
print('  伤害增量分布:', dict(deltas.most_common()))
print('  格挡增量分布:', dict(bdeltas.most_common()))
print('  升级降费的牌数:', costdn)

# ---- 原版 Rare/Common 的复合牌强度参考 ----
print()
print('=' * 78)
print('原版基础牌（Basic）')
print('=' * 78)
for c in vanilla:
    if c.get('rarity') == 'Basic':
        print('  %-20s cost=%-3s %-8s %s' % (c['name'], c.get('cost'), c.get('type'), c.get('vars')))

print()
print('=' * 78)
print('万界如林 全部卡牌')
print('=' * 78)
for c in sorted(mod, key=lambda x: (x.get('cost', 'z'), x.get('type', ''), x['name'])):
    if 'fail' in c:
        print('  %-20s FAIL %s' % (c['name'], c['fail'][:90])); continue
    up = ''
    if c.get('up_vars'):
        diff = {k: (c['vars'].get(k), v) for k, v in c['up_vars'].items() if c['vars'].get(k) != v}
        if diff: up = '  UP:' + ','.join(f'{k} {a}->{b}' for k, (a, b) in diff.items())
    print('  %-20s cost=%-3s %-8s %-10s %-9s %s%s' % (
        c['name'], c.get('cost'), c.get('type'), c.get('rarity'), c.get('target'),
        c.get('vars'), up))
