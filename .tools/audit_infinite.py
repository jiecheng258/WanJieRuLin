# -*- coding: utf-8 -*-
r"""反无限审计 —— 把「哪些牌能拼成无限循环」固化成检查项。

背景
----
曾经 3 张牌就能拼出无限，双方各自看都没问题、合起来才是无限：

    层层侵蚀（0 费产鬼气） → 审时度势（0 费：X 鬼气 → X 能量 + X 张牌） → 抽回层层侵蚀

问题**不在单张牌，而在组合** —— 人眼很难发现，所以固化成机器检查。

铁律 R1–R8（详见 handoff 技能「五·补」）
----------------------------------------
R1  0 费牌不产能量、不产鬼气
R2  「鬼气 → 能量」的兑换器必须 [消耗]
R3  「鬼气 → 抽牌」的牌不产能量
R4  抽牌 ≥2 的牌必须 ≥1 费，或 [消耗]
R5  触发式收益（能力）必须有每回合上限
R6  0 费产鬼气必须 [消耗]
R7  单卡不同时「抽牌 + 产能量」
R8  「本回合」增幅类不产可循环资源（人工复核项，脚本不拦）

用法
----
    python audit_infinite.py             # 扫描并打印
    python audit_infinite.py --self-test # 顺带验证规则本身能命中已知坏例子
"""
import io
import os
import re
import sys

CODE = r'C:\Users\wangx\Documents\Default Project\WanJieRuLin\WanJieRuLinCode'
CARDS = os.path.join(CODE, 'Cards')
POWERS = os.path.join(CODE, 'Powers')

# ============================================================================
# 解析
# ============================================================================

RE_CTOR = re.compile(
    r'base\(\s*(-?\d+)\s*,\s*CardType\.(\w+),\s*CardRarity\.(\w+)')
RE_GQ_FIXED = re.compile(r'SetGhostQiCost\(\s*(\d+)\s*\)')
RE_GQ_X = re.compile(r'SetGhostQiCostX\s*\(')
RE_ENERGY_X = re.compile(r'HasEnergyCostX\s*=>\s*true')

RE_EXHAUST = re.compile(r'CardKeyword\.Exhaust')

# 抽牌：Draw(ctx, N) —— N 可能是数字，也可能是 DynamicVars.Cards.IntValue 之类
RE_DRAW = re.compile(r'\bDraw\s*\([^;]*?,\s*([^,)]+)\)')
# 产能量
RE_ENERGY = re.compile(r'GainEnergy\s*\(\s*([^)]+?)\s*\)')
# 产鬼气
RE_QI_GAIN = re.compile(r'\bGainGhostQi\s*\(|GhostQi\.Gain\s*\(')
# 失鬼气（含清空 / 设为）
RE_QI_LOSE = re.compile(r'\bLoseGhostQi\s*\(|ClearGhostQi\s*\(|GhostQi\.Lose\s*\(|GhostQi\.SpendAll\s*\(')


def _int_or_var(expr):
    """把 '3' 解析成 3，把 'DynamicVars...' 之类的解析成字符串 'var'。"""
    e = expr.strip()
    if re.fullmatch(r'\d+', e):
        return int(e)
    return 'var'


def parse_card(path):
    s = io.open(path, encoding='utf-8').read()
    name = os.path.basename(path)[:-3]
    m = RE_CTOR.search(s)
    if not m:
        return None
    # 只在 OnPlay 体内找效果（避免把 CanonicalVars / 注释算进去）
    i = s.find('OnPlay')
    body = s[i:] if i >= 0 else s

    draw_vals = [_int_or_var(x) for x in RE_DRAW.findall(body)]
    energy_vals = [_int_or_var(x) for x in RE_ENERGY.findall(body)]

    return dict(
        name=name,
        cost=int(m.group(1)),
        ctype=m.group(2),
        rarity=m.group(3),
        cost_x=bool(RE_ENERGY_X.search(s)),
        gq_cost=int(RE_GQ_FIXED.search(s).group(1)) if RE_GQ_FIXED.search(s) else (0 if not RE_GQ_X.search(s) else 'X'),
        exhaust=bool(RE_EXHAUST.search(s)),
        draw_max=max([v for v in draw_vals if isinstance(v, int)] or [0]),
        draw_any=bool(draw_vals),
        draw_var=any(v == 'var' for v in draw_vals),
        energy_max=max([v for v in energy_vals if isinstance(v, int)] or [0]),
        energy_any=bool(energy_vals),
        qi_gain=bool(RE_QI_GAIN.search(body)),
        qi_lose=bool(RE_QI_LOSE.search(body)),
        path=path,
    )


# ============================================================================
# 规则
# ============================================================================

# 触发式能力的钩子：会跟随玩家的资源变动反复触发
RE_HOOK = re.compile(
    r'AfterSecondaryResourceChanged|AfterSecondaryResourceSpent|AfterSecondaryResourceReset|'
    r'AfterPlayerTurnStart|AfterSideTurnEnd|'
    r'ModifyEnergyGain|AfterModifyingEnergyGain')
# 每回合上限的写法痕迹
RE_CAP = re.compile(
    r'MaxTriggersPerTurn|_triggersThisTurn|_blockThisTurn|'
    r'PerTurn|per turn|每回合')


def check_card(r):
    """返回 [(规则号, 说明), ...]"""
    out = []

    if r['cost'] == 0 and not r['exhaust']:
        if r['energy_any']:
            out.append(('R1', '0 费却产能量 —— 0 费产资源是无限的第一因'))
        if r['qi_gain']:
            out.append(('R6', '0 费却产鬼气 —— 白产燃料；要么改 ≥1 费，要么 [消耗]'))

    if r['draw_any'] and not r['exhaust'] and r['draw_max'] >= 2 and r['cost'] == 0:
        out.append(('R4', '抽牌 ≥2 却是 0 费且不 [消耗] —— 能把自己抽回来'))

    if r['draw_any'] and r['energy_any'] and not r['exhaust']:
        out.append(('R7', '同一张牌既抽牌又产能量 —— 净资源为正'))

    if r['qi_lose'] and r['energy_any'] and not r['exhaust']:
        if r['draw_any']:
            out.append(('R3', '鬼气换能量**且**换牌，还不 [消耗] —— 净赚手牌'))
        else:
            out.append(('R2', '鬼气换能量的兑换器没有 [消耗] —— 会被循环利用'))

    return out


def check_power(path):
    """能力牌的 R5：有资源类钩子 + 给资源，但没有每回合上限。

    ★ 排除「一次性能力」：如果能力自己会 PowerCmd.Remove(this)，
      它本来就是一次性的（例如「下回合给资源」类），不需要每回合上限 ——
      这类是误报，必须放过，否则审计会一直挂着噪音。
    """
    s = io.open(path, encoding='utf-8').read()
    name = os.path.basename(path)[:-3]
    if 'Power' not in name:
        return []
    if 'PowerCmd.Remove(this)' in s:
        return []
    if not RE_HOOK.search(s):
        return []
    gives = bool(RE_DRAW.search(s) or RE_ENERGY.search(s) or RE_QI_GAIN.search(s))
    if not gives:
        return []
    if RE_CAP.search(s):
        return []
    return [(name, 'R5', '触发式收益没有每回合上限 —— 配任意产气牌即无限')]


def scan():
    card_files = [os.path.join(CARDS, f) for f in sorted(os.listdir(CARDS))
                  if f.endswith('.cs') and f != 'WanJieRuLinCardModel.cs']
    power_files = [os.path.join(POWERS, f) for f in sorted(os.listdir(POWERS))
                   if f.endswith('.cs')]

    hits = []
    n_card = 0
    for p in card_files:
        r = parse_card(p)
        if r is None:
            continue
        n_card += 1
        for rule, msg in check_card(r):
            hits.append((r['name'], rule, msg, r))
    for p in power_files:
        for h in check_power(p):
            hits.append((h[0], h[1], h[2], None))
    return n_card, len(power_files), hits


# ============================================================================
# 自检：规则必须能命中已知坏例子、且放行干净例子
# ============================================================================

BAD = {
    '审时度势（原版元凶）': '''
public sealed class ShenShiDuoShi : WanJieRuLinCardModel
{
    public ShenShiDuoShi() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        SetGhostQiCostX();
    }
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    {
        var x = GhostQiXValue(p);
        await ClearGhostQi();
        await GainEnergy(x);
        await Draw(c, x);
    }
}''',
    '藏锋（原版元凶）': '''
public sealed class CangFeng : WanJieRuLinCardModel
{
    public CangFeng() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        SetGhostQiCost(1);
    }
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    {
        await Draw(c, 2);
        await GainEnergy(1);
    }
}''',
    '层层侵蚀（0 费产气）': '''
public sealed class CengCengQinShi : WanJieRuLinCardModel
{
    public CengCengQinShi() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    {
        await GainGhostQi(2);
        await Draw(c, 1);
    }
}''',
}

CLEAN = {
    '宿墨（1 费产气，正常）': '''
public sealed class SuMo : WanJieRuLinCardModel
{
    public SuMo() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    {
        await DealDamage(c, p.Target!, 7m);
        await GainGhostQi(2);
    }
}''',
    '破砚（收紧后：0 费但 [消耗]）': '''
public sealed class PoYan : WanJieRuLinCardModel
{
    public PoYan() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    {
        var spent = await ClearGhostQi();
        await GainEnergy(spent / 4);
        await Draw(c, 1);
    }
}''',
    '留白（1 费抽 2，正常）': '''
public sealed class LiuBai : WanJieRuLinCardModel
{
    public LiuBai() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    {
        await Draw(c, 2);
    }
}''',
}


def self_test():
    print('--- 规则自检（坏例子：应当命中）---')
    bad_ok = True
    for label, src in BAD.items():
        tmp = os.path.join(os.environ.get('TEMP', '.'), '_ai_bad.cs')
        io.open(tmp, 'w', encoding='utf-8').write(src)
        r = parse_card(tmp)
        got = check_card(r) if r else []
        os.remove(tmp)
        tag = 'HIT ' if got else 'MISS'
        if not got:
            bad_ok = False
        print('  %s %-26s -> %s' % (tag, label, [g[0] for g in got]))

    print('--- 规则自检（干净例子：应当放行）---')
    clean_ok = True
    for label, src in CLEAN.items():
        tmp = os.path.join(os.environ.get('TEMP', '.'), '_ai_ok.cs')
        io.open(tmp, 'w', encoding='utf-8').write(src)
        r = parse_card(tmp)
        got = check_card(r) if r else []
        os.remove(tmp)
        tag = 'clean' if not got else 'HIT  '
        if got:
            clean_ok = False
        print('  %s %-26s -> %s' % (tag, label, [g[0] for g in got]))

    print()
    if bad_ok and clean_ok:
        print('自检通过：坏例子全部命中、干净例子全部放行。')
    else:
        print('!! 自检失败：坏例子漏报=%s，干净例子误报=%s' % (not bad_ok, not clean_ok))
    return bad_ok and clean_ok


def main():
    n_card, n_power, hits = scan()

    print('=' * 72)
    print('反无限审计（R1–R8）')
    print('=' * 72)
    print('扫描：卡牌 %d 张、能力 %d 个' % (n_card, n_power))
    print()

    if not hits:
        print('通过：未发现可拼成无限循环的部件。')
    else:
        by_rule = {}
        for name, rule, msg, _ in hits:
            by_rule.setdefault(rule, []).append((name, msg))
        print('发现 %d 处可疑：\n' % len(hits))
        for rule in sorted(by_rule):
            print('[%s] %d 处' % (rule, len(by_rule[rule])))
            for name, msg in by_rule[rule]:
                print('    %-22s %s' % (name, msg))
            print()

    if '--self-test' in sys.argv:
        print()
        ok = self_test()
        return 0 if (ok and not hits) else 1
    return 0 if not hits else 1


if __name__ == '__main__':
    sys.exit(main())
