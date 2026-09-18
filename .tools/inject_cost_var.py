# -*- coding: utf-8 -*-
"""把 GhostQiCostVarOf(N) 注入到各张固定鬼气费用卡的 CanonicalVars 里。

这样卡面文本可以用 {GhostQiCost:secondaryResourceIcons()}
渲染成「鬼气图标 + 数字」，而且数值是真正的 DynamicVar（升级能用 :diff()）。
"""
import io, os, re, sys

CODE = r'C:\Users\wangx\Documents\Default Project\WanJieRuLin\WanJieRuLinCode\Cards'

# 固定费用的卡（X 费用卡不需要数字变量；GuiYu 单独处理，因为升级会改费用）
FIXED = {
    'AoMan': 2,
    'CengCengQinShi': 3,
    'ChiXuQinRao': 1,
    'HeiAnBiZhang': 1,
    'PiaoMiaoJianJue': 2,
    'QinShi': 1,
    'ShenShengBaoZou': 2,
    'ShengWenZi': 1,
    'ShiTong': 3,
    'XiaoHuo': 1,
    'YaZhiYuWang': 1,
    'YinYangGeHunXiao': 2,
    'YouRanGuiHuo': 3,
    'YouYouDianLong': 3,
}

CV = re.compile(r'(CanonicalVars\s*=>\s*\[)(\s*)')


def main():
    changed, skipped, problems = [], [], []
    for f in sorted(os.listdir(CODE)):
        if not f.endswith('.cs'):
            continue
        cls = f[:-3]
        if cls not in FIXED:
            continue
        p = os.path.join(CODE, f)
        src = io.open(p, encoding='utf-8', errors='replace').read()

        if 'GhostQiCostVarOf' in src:
            skipped.append(cls + ' (已有)')
            continue

        m = CV.search(src)
        if not m:
            problems.append(cls + ' (找不到 CanonicalVars => [ )')
            continue
        if not re.search(r'class\s+' + re.escape(cls) + r'\s*:\s*WanJieRuLinCardModel', src):
            problems.append(cls + ' (不是 WanJieRuLinCardModel 子类)')
            continue

        n = FIXED[cls]
        # 在 '[' 之后插入一行
        insert = '\n        // 鬼气费用（卡面用 {GhostQiCost:secondaryResourceIcons()} 渲染成图标+数字）。\n        GhostQiCostVarOf(%d),' % n
        new = src[:m.end(1)] + insert + src[m.end(1):]
        io.open(p, 'w', encoding='utf-8', newline='\n').write(new)
        changed.append('%s -> GhostQiCostVarOf(%d)' % (cls, n))

    print('=== 已注入 %d 张 ===' % len(changed))
    for c in changed:
        print('  ', c)
    if skipped:
        print('=== 跳过 %d 张 ===' % len(skipped))
        for s in skipped:
            print('  ', s)
    if problems:
        print('=== 需要处理 %d 张 ===' % len(problems))
        for s in problems:
            print('  !!', s)
    return 1 if problems else 0


if __name__ == '__main__':
    sys.exit(main())
