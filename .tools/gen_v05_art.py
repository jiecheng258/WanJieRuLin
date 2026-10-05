# -*- coding: utf-8 -*-
r"""为 v0.5 的 90 张卡 + 9 个新能力程序化生成水墨风美术（零积分）。

用「点/线/面」三类决定主色调，让玩家一眼能认出归属：
    点 → 青蓝（清透、轻）
    线 → 墨绿（沉稳、拉伸）
    面 → 紫红（浓烈、厚重）
"""
import io, os, re, math, random
from PIL import Image, ImageDraw, ImageFilter, ImageEnhance

REPO = r'C:\Users\wangx\Documents\Default Project\WanJieRuLin'
IMG = os.path.join(REPO, 'WanJieRuLin', 'images')
GEN = r'C:\Users\wangx\WorkBuddy\_scratch\sts2-mod\gen_cards_v05.py'

src = io.open(GEN, encoding='utf-8').read()
rows = re.findall(
    r"card\('(\w+)',\s*'([^']*)',\s*(-?\d+),\s*'(\w+)',\s*'(\w+)',\s*'(\w+)'", src)
print('规格卡数:', len(rows))

TINT = {
    'Point': (58, 122, 168),      # 青蓝
    'Line': (54, 128, 108),       # 墨绿
    'Face': (128, 62, 132),       # 紫红
    'None': (86, 78, 104),        # 中性灰紫
}
# 抓每张卡的 aspect
asp = {}
for m in re.finditer(r"card\('(\w+)'.*?aspect='(\w+)'", src, re.S):
    asp[m.group(1)] = m.group(2)


def art(seed, tint, w=500, h=380, style='card'):
    r = random.Random(seed)
    im = Image.new('RGB', (w, h), (26, 24, 32))
    d = ImageDraw.Draw(im, 'RGBA')
    tr, tg, tb = tint
    # 远山层
    for layer in range(5):
        base = h * (0.34 + layer * 0.13)
        amp = 58 - layer * 9
        pts = [(x, base + math.sin(x * 0.010 + layer * 1.9 + seed * 0.01) * amp
                + r.randint(-7, 7)) for x in range(0, w + 24, 22)]
        pts += [(w, h), (0, h)]
        d.polygon(pts, fill=(tr + layer * 10, tg + layer * 10, tb + layer * 8,
                             34 + layer * 30))
    # 飞白
    for _ in range(70):
        x, y = r.randint(0, w), r.randint(0, int(h * 0.85))
        rr = r.randint(2, 15)
        d.ellipse([x, y, x + rr, y + rr],
                  fill=(235, 228, 246, r.randint(26, 120)))
    im = im.filter(ImageFilter.GaussianBlur(0.7))
    # 底部压暗，方便压字
    for i in range(80):
        d.line([(0, h - i), (w, h - i)], fill=(20, 17, 26, 5))
    return im


def icon(seed, tint, size=128):
    r = random.Random(seed)
    im = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(im, 'RGBA')
    tr, tg, tb = tint
    d.ellipse([6, 6, size - 6, size - 6], fill=(tr // 3, tg // 3, tb // 3, 240),
              outline=(tr, tg, tb, 255), width=3)
    for _ in range(30):
        x, y = r.randint(16, size - 16), r.randint(16, size - 16)
        rr = r.randint(2, 8)
        d.ellipse([x, y, x + rr, y + rr], fill=(228, 216, 246, r.randint(70, 210)))
    return im


made = 0
for cls, cn, cost, typ, rar, tgt in rows:
    a = asp.get(cls, 'None')
    tint = TINT.get(a, TINT['None'])
    seed = abs(hash(cls)) % 100000
    art(seed, tint).save(os.path.join(IMG, 'cards', cls + '.png'), 'PNG', optimize=True)
    made += 1
print('卡图 %d 张' % made)

NEW_POWERS = ['BiFengPower', 'LiDaoPower', 'MoYunPower', 'RunBiPower', 'LiTouPower',
              'PoTianPower', 'ZongHengPower', 'WuShiWuZhongPower', 'HuiFengTracePower']
for i, cls in enumerate(NEW_POWERS):
    tint = TINT['Point' if i % 3 == 0 else ('Line' if i % 3 == 1 else 'Face')]
    icon(abs(hash(cls)) % 100000, tint).save(
        os.path.join(IMG, 'powers', cls + '.png'), 'PNG', optimize=True)
print('能力图标 %d 个' % len(NEW_POWERS))
print('done')
