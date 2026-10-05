# -*- coding: utf-8 -*-
r"""v0.6 数值上调：全部伤害/格挡 ×1.2（对齐「略高于原版」）。

依据：用户朋友实测「强度低于原作角色一半以上」。
    原版中位数（1费攻击 7 / 2费 13 / 3费 22；1费格挡 6 / 2费 11）
    本模组目标 = 原版 × 1.2（线的档位），点更低、面更高。

只上调伤害/格挡与「奖励类」命名变量，不动费用、不动 Repeat/计数类。
"""
import io, re, math

P = r'C:\Users\wangx\WorkBuddy\_scratch\sts2-mod\gen_cards_v05.py'
s = io.open(P, encoding='utf-8').read()
orig = s

FACTOR = 1.2
# 需要上调的命名变量（浮动奖励类）
BONUS_VARS = ['ThinBonus', 'HighBonus', 'EvenBonus', 'EdgeBonus', 'Bonus',
              'BonusDamage', 'BonusPerStep', 'BonusPer', 'BonusPerQi',
              'BonusStat', 'BonusFree', 'BonusDraw', 'PerEdge', 'PerForce',
              'PerMoYun', 'PerStep', 'EmptyBonus', 'StrengthLoss',
              'Vulnerable', 'Weak', 'BlockPerQi', 'PerQi', 'PerCard',
              'PenaltyStep', 'QiSwing']


def up(n, f=FACTOR):
    v = int(n) * f
    # 小数值向上取整（避免 3×1.2=3.6 → 3 的「白涨」）
    return int(math.ceil(v - 1e-9)) if v < 10 else int(round(v))


# 1) DV % N / BV % N
n_dv = n_bv = 0
def rep_dv(m):
    global n_dv
    n_dv += 1
    return 'DV %% %d' % up(m.group(1))
s = re.sub(r'DV % (\d+)', rep_dv, s)

def rep_bv(m):
    global n_bv
    n_bv += 1
    return 'BV %% %d' % up(m.group(1))
s = re.sub(r'BV % (\d+)', rep_bv, s)

# 2) 命名变量
n_nv = 0
for name in BONUS_VARS:
    pat = re.compile(r"vi\('%s',\s*(\d+)\)" % name)

    def rep(m, _n=name):
        global n_nv
        n_nv += 1
        return "vi('%s', %d)" % (_n, up(m.group(1)))
    s = pat.sub(rep, s)

# 3) 保护：Meta 用到的固定数（费用、条件阈值）不能被改
#    PlayCondition 里的 LiDaoAtLeast(5) / HandCountAtMost(0) 等不在 DV/BV/vi 范围内 ✓
io.open(P, 'w', encoding='utf-8', newline='\n').write(s)

import ast
ast.parse(io.open(P, encoding='utf-8').read())
print('上调完成：DV %d 处 / BV %d 处 / 命名变量 %d 处' % (n_dv, n_bv, n_nv))
print()
print('=== 抽样核对 ===')
for cls in ('DaJi', 'HengXian', 'ChengFu', 'PoMian', 'DaPoMo', 'ShanGaoShuiChang', 'HuiFeng'):
    i = s.find("card('%s'" % cls)
    if i < 0:
        continue
    seg = s[i:i + 400]
    m = re.search(r'vars_=\[(.*?)\]', seg, re.S)
    print('  %-16s %s' % (cls, (m.group(1).strip().replace('\n', ' ') if m else '?')[:80]))
