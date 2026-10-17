# -*- coding: utf-8 -*-
"""
v0.9 美术生成：从 AI 水墨背景图生成 90 张卡图 + 2 遗物图 + 36 能力图。
源图 = _scratch\sts2-mod\ai_bg（5 张山水）+ ai_ink_v04（5 张抽象水墨）。
按稀有度/类型/主题分配源图/取景/色调，保证每张图独立且贴题。
"""
import os
from PIL import Image, ImageDraw, ImageFilter, ImageEnhance, ImageChops

BG = r'C:\Users\wangx\WorkBuddy\_scratch\sts2-mod\ai_bg'
INK4 = r'C:\Users\wangx\WorkBuddy\_scratch\sts2-mod\ai_ink_v04'
OUT = r'C:\Users\wangx\Documents\Default Project\WanJieRuLin\WanJieRuLin\images'

SRC = {
    'a': os.path.join(BG,   'Chinese_ink_wash_painting_back_2026-09-27T06-27-02.png'),
    'b': os.path.join(BG,   'Chinese_ink_wash_painting_back_2026-09-27T06-27-07.png'),
    'c': os.path.join(BG,   'Chinese_ink_wash_painting_back_2026-09-27T06-27-09.png'),
    'd': os.path.join(BG,   'Chinese_ink_wash_painting_back_2026-09-27T06-29-38.png'),
    'e': os.path.join(BG,   'Chinese_ink_wash_painting__sum_2026-09-27T06-27-07.png'),
    'f': os.path.join(INK4, 'Abstract_Chinese_ink_wash_pain_2026-09-28T06-23-21.png'),
    'g': os.path.join(INK4, 'Abstract_Chinese_ink_wash_pain_2026-09-28T06-23-40.png'),
    'h': os.path.join(INK4, 'Abstract_Chinese_ink_wash_pain_2026-09-28T06-23-59.png'),
    'i': os.path.join(INK4, 'Abstract_Chinese_ink_wash_pain_2026-09-28T06-24-22.png'),
    'j': os.path.join(INK4, 'Abstract_Chinese_ink_wash_pain_2026-09-28T06-24-46.png'),
}

CARD_W, CARD_H = 500, 380
SIZE = 128

# 调色：INK 幽紫主色 / BLOOD 攻击 / JADE 鬼气资源 / GOLD 稀有先古 / PALE 防御技能 / DARK 消耗
INK   = (0.70, 0.52, 0.98)
BLOOD = (0.86, 0.28, 0.34)
JADE  = (0.42, 0.84, 0.72)
GOLD  = (0.98, 0.80, 0.42)
PALE  = (0.86, 0.84, 0.92)
DARK  = (0.18, 0.14, 0.28)


def tint(img, rgb, s=0.30):
    r, g, b = [int(c * 255) for c in rgb]
    return Image.blend(img, Image.new('RGB', img.size, (r, g, b)), s)


def crop_to(img, w, h, fx=0.5, fy=0.42, zoom=1.0):
    iw, ih = img.size
    target, cur = w / h, iw / ih
    nw, nh = (int(ih*target), ih) if cur > target else (iw, int(iw/target))
    nw, nh = int(nw/zoom), int(nh/zoom)
    x = max(0, min(int((iw-nw)*fx), iw-nw)); y = max(0, min(int((ih-nh)*fy), ih-nh))
    return img.crop((x, y, x+nw, y+nh)).resize((w, h), Image.LANCZOS)


def vignette(img, p=0.55):
    w, h = img.size
    mask = Image.new('L', (w, h), 0); d = ImageDraw.Draw(mask)
    d.ellipse((-w*.25, -h*.30, w*1.25, h*1.30), fill=255)
    mask = mask.filter(ImageFilter.GaussianBlur(min(w, h)*.18))
    return Image.composite(img, ImageEnhance.Brightness(img).enhance(1-p), mask)


def grain(img, amount=6):
    w, h = img.size
    noise = Image.effect_noise((w, h), amount).convert('L').filter(ImageFilter.GaussianBlur(.6))
    return ImageChops.overlay(img, Image.merge('RGB', (noise, noise, noise)))


def glow(img, rgb, radius=18, strength=.4):
    w, h = img.size
    r, g, b = [int(c*255) for c in rgb]
    halo = Image.new('RGB', (w, h), (r, g, b))
    mask = Image.new('L', (w, h), 0); d = ImageDraw.Draw(mask)
    m = int(min(w, h)*.10)
    d.rectangle((m, m, w-m, h-m), outline=255, width=max(4, m//2))
    mask = mask.filter(ImageFilter.GaussianBlur(radius))
    return Image.composite(Image.blend(img, halo, strength), img, mask)


def ink_wash(img, rgb, strength=.5, corner='tl'):
    w, h = img.size
    r, g, b = [int(c*255) for c in rgb]
    grad = Image.new('L', (w, h), 0); d = ImageDraw.Draw(grad)
    for i in range(60):
        t = i/60; alpha = int(255*(1-t)**2)
        if corner == 'tl':   d.polygon([(0, 0), (w*t, 0), (0, h*t)], fill=alpha)
        elif corner == 'br': d.polygon([(w, h), (w-w*t, h), (w, h-h*t)], fill=alpha)
        elif corner == 'tr': d.polygon([(w, 0), (w-w*t, 0), (w, h*t)], fill=alpha)
        else:                d.polygon([(0, h), (w*t, h), (0, h-h*t)], fill=alpha)
    grad = grad.filter(ImageFilter.GaussianBlur(18))
    return Image.composite(Image.new('RGB', (w, h), (r, g, b)), img,
                           grad.point(lambda v: int(v*strength)))


def make(src, fx, fy, zoom, rgb, ts, bright, sat, size, corner, vig, extra_glow=False):
    img = crop_to(SRCS[src], size[0], size[1], fx, fy, zoom)
    img = tint(img, rgb, ts)
    img = ImageEnhance.Color(img).enhance(sat)
    img = ImageEnhance.Brightness(img).enhance(bright)
    img = ImageEnhance.Contrast(img).enhance(1.08)
    img = vignette(img, vig)
    img = ink_wash(img, rgb, .5, corner)
    if extra_glow:
        img = glow(img, rgb)
    return grain(img, 5)


def save(img, path):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    img.save(path, 'PNG', optimize=True)


print('loading sources...')
SRCS = {k: Image.open(p).convert('RGB') for k, p in SRC.items()}
print('  loaded', len(SRCS))

# ============================================================
# 90 张卡：cls -> (src, fx, fy, zoom, 主色, tint强度, 明度, 饱和, corner, vig)
# 分配原则：
#   攻击→BLOOD、技能/防御→PALE、鬼气相关→JADE、Rare/Ancient→GOLD/INK、
#   Basic→INK、消耗牌→DARK 系，源图 a-e 山水 / f-j 抽象水墨轮换，取景散开
# ============================================================
CARDS = {}
def c(cls, src, fx, fy, zoom, rgb, ts, bright, sat, corner='tl', vig=.55):
    CARDS[cls] = (src, fx, fy, zoom, rgb, ts, bright, sat, corner, vig)

# ---- Basic 3（幽紫主调）----
c('DaJi',    'a', .72, .30, 1.30, INK, .18, 1.02, 1.05)
c('FangYu',  'c', .40, .36, 1.25, PALE, .18, 1.06, .92, 'br')
c('ZuoSui',  'd', .55, .42, 1.55, INK, .40, .92, 1.20, 'br', .66)

# ---- Common 攻击 8（血色调）----
c('MoZhan',   'a', .30, .44, 1.42, BLOOD, .30, .98, 1.14)
c('PoFeng',   'b', .26, .34, 1.38, JADE,  .26, 1.02, 1.10)
c('GuiYa',    'd', .30, .52, 1.52, BLOOD, .36, .94, 1.18, 'br', .64)
c('SiGuang',  'f', .30, .30, 1.44, PALE,  .26, 1.06, 1.00, 'tr')
c('MoHen',    'c', .28, .48, 1.48, BLOOD, .30, .96, 1.12, 'br')
c('ChanYing', 'g', .26, .44, 1.50, DARK,  .34, .92, 1.16, 'br', .66)
c('YinXi',    'b', .68, .40, 1.40, DARK,  .30, .94, 1.12, 'tr')
c('LianMo',   'h', .28, .38, 1.46, BLOOD, .32, .96, 1.14)

# ---- Common 技能 8（淡色调）----
c('MoDun',   'c', .62, .36, 1.36, PALE, .22, 1.04, .94)
c('JuQi',    'e', .30, .40, 1.40, JADE, .26, 1.00, 1.08)
c('HuiMo',   'f', .62, .40, 1.42, PALE, .24, 1.06, .96, 'tr')
c('ChengXi', 'b', .44, .32, 1.32, PALE, .20, 1.06, .94)
c('YinQi',   'g', .58, .34, 1.44, JADE, .30, 1.00, 1.10, 'tr')
c('GuiBi',   'd', .68, .34, 1.44, DARK, .32, .92, 1.14, 'tr', .64)
c('MoYun',   'h', .62, .44, 1.48, JADE, .28, .98, 1.08, 'br')
c('MoYi',    'i', .30, .42, 1.46, PALE, .26, 1.02, .98, 'br')

# ---- Common 能力 4 ----
c('GuiMoJia', 'd', .40, .30, 1.60, DARK, .34, .94, 1.14)
c('LingLi',   'a', .44, .36, 1.36, BLOOD, .28, 1.00, 1.10, 'tr')
c('XinMo',    'g', .34, .50, 1.54, JADE, .30, .98, 1.12, 'tl')
c('ZhanYi',   'b', .56, .46, 1.38, BLOOD, .28, 1.00, 1.10, 'br')

# ---- Uncommon 攻击 12 ----
c('MoLong',   'a', .58, .26, 1.36, INK, .32, .96, 1.14, 'tr', .60)
c('SuiFeng',  'b', .24, .28, 1.34, PALE, .22, 1.04, .98, 'tl')
c('LianZhan', 'c', .70, .44, 1.44, BLOOD, .32, .96, 1.14, 'tr')
c('HanMo',    'd', .26, .36, 1.54, JADE, .30, .94, 1.12, 'tl', .62)
c('MoSi',     'f', .52, .34, 1.48, JADE, .28, .98, 1.10, 'tr')
c('ChiXue',   'h', .30, .52, 1.56, BLOOD, .38, .92, 1.18, 'tl', .68)
c('YingDian', 'i', .26, .32, 1.50, DARK, .36, .90, 1.18, 'br', .68)
c('GuiLie',   'e', .30, .52, 1.56, BLOOD, .36, .92, 1.18, 'br', .66)
c('MoChao',   'c', .32, .30, 1.42, INK, .32, .96, 1.12, 'tl', .60)
c('PoKong',   'g', .62, .30, 1.46, BLOOD, .30, .98, 1.12, 'tr')
c('XuShi',    'i', .58, .42, 1.44, PALE, .26, 1.00, 1.00, 'tl')
c('ChiGui',   'j', .28, .44, 1.54, BLOOD, .36, .92, 1.18, 'br', .68)

# ---- Uncommon 技能 13 ----
c('YinBi',    'b', .66, .34, 1.38, PALE, .22, 1.04, .94, 'tr')
c('MoQi',     'e', .56, .36, 1.42, JADE, .28, .98, 1.08, 'br')
c('HuiHun',   'd', .44, .52, 1.58, DARK, .38, .90, 1.16, 'br', .68)
c('NingMo',   'f', .58, .46, 1.50, PALE, .24, 1.02, .98, 'tl')
c('TuoYing',  'g', .30, .36, 1.46, DARK, .30, .92, 1.12, 'tl', .64)
c('AnXiang',  'h', .58, .34, 1.44, PALE, .26, 1.02, .98, 'br')
c('MoHua',    'i', .56, .30, 1.44, JADE, .28, 1.00, 1.08, 'tr')
c('XunFeng',  'a', .26, .34, 1.36, PALE, .22, 1.04, .96, 'br')
c('MingJing', 'j', .58, .40, 1.46, PALE, .24, 1.04, .96, 'tl')
c('JieYin',   'b', .32, .50, 1.44, INK, .28, .98, 1.08, 'tl')
c('GuiZhao',  'd', .62, .44, 1.50, BLOOD, .32, .94, 1.14, 'br')
c('MoWu',     'e', .60, .40, 1.40, PALE, .24, 1.02, .98, 'tr')
c('HuiMoLian','j', .56, .52, 1.54, DARK, .36, .90, 1.16, 'tr', .68)

# ---- Uncommon 能力 12 ----
c('GuiMoXin',  'g', .40, .40, 1.58, JADE, .32, .96, 1.12)
c('DianZeng',  'f', .34, .44, 1.52, GOLD, .28, 1.00, 1.08, 'tr')
c('XianZeng',  'h', .60, .34, 1.50, GOLD, .28, 1.00, 1.08, 'br')
c('MianZeng',  'i', .32, .50, 1.52, GOLD, .28, 1.00, 1.08, 'tl')
c('ZuoSuiZhang','d', .34, .34, 1.60, INK, .36, .94, 1.14, 'br', .64)
c('GuiXi',     'e', .36, .44, 1.50, JADE, .30, .98, 1.10, 'tr')
c('MoYin',     'j', .32, .36, 1.52, INK, .32, .94, 1.12, 'tl', .64)
c('LingHun',   'h', .34, .50, 1.56, DARK, .34, .92, 1.14, 'tr', .66)
c('XuShiJie',  'i', .60, .48, 1.50, PALE, .26, .98, 1.00, 'br')
c('GuiYing',   'f', .60, .40, 1.52, DARK, .32, .92, 1.14, 'tr', .66)
c('CanYing',   'g', .58, .52, 1.54, DARK, .34, .90, 1.16, 'tl', .68)
c('MoXin',     'j', .58, .30, 1.50, JADE, .30, .96, 1.10, 'br')

# ---- Rare 攻击 8（金紫色调）----
c('WanJieZhan',     'a', .50, .40, 1.50, GOLD, .34, .98, 1.16, 'tl', .64)
c('MoRanJiangShan', 'a', .50, .28, 1.30, INK, .36, .94, 1.16, 'tl', .62)
c('PoXiao',         'b', .46, .26, 1.42, GOLD, .30, 1.00, 1.10, 'tl')
c('LingXiZhan',     'c', .44, .34, 1.44, GOLD, .30, .98, 1.12, 'tr')
c('XueGui',         'h', .34, .40, 1.58, BLOOD, .40, .90, 1.20, 'tl', .70)
c('MoHai',          'd', .52, .34, 1.48, INK, .34, .94, 1.14, 'br', .62)
c('GuiShen',        'e', .32, .38, 1.58, BLOOD, .38, .92, 1.18, 'br', .68)
c('MoJie',          'j', .60, .34, 1.54, DARK, .38, .90, 1.18, 'tl', .70)

# ---- Rare 技能 10 ----
c('WanJieBi',  'c', .60, .28, 1.40, PALE, .26, 1.00, .98, 'tl')
c('XuShen',    'f', .36, .34, 1.52, PALE, .26, 1.00, 1.00, 'tr')
c('MoYuan',    'g', .34, .40, 1.54, JADE, .32, .94, 1.12, 'br', .66)
c('LingXi',    'i', .32, .36, 1.54, GOLD, .30, .98, 1.10, 'br')
c('HuiTian',   'j', .34, .46, 1.56, GOLD, .32, .96, 1.12, 'tl', .66)
c('MoWuJue',   'e', .62, .34, 1.46, PALE, .26, 1.00, 1.00, 'br')
c('WanNian',   'f', .62, .48, 1.56, JADE, .32, .94, 1.12, 'tl', .66)
c('LingXu',    'h', .62, .30, 1.52, GOLD, .30, .98, 1.10, 'tr')
c('MoSheng',   'i', .36, .44, 1.52, PALE, .26, .98, 1.00, 'tl')
c('ZuoSuiYin', 'g', .60, .42, 1.54, INK, .34, .94, 1.12, 'br', .64)

# ---- Rare 能力 8 ----
c('MoShen',     'e', .50, .30, 1.56, GOLD, .34, .98, 1.14, 'tl', .64)
c('GuiLuo',     'd', .58, .40, 1.58, INK, .38, .92, 1.16, 'tr', .66)
c('WanJieLin',  'a', .50, .46, 1.55, GOLD, .38, .94, 1.16, 'tl', .68)
c('XuKongJie',  'f', .62, .34, 1.54, DARK, .34, .92, 1.14, 'tr', .66)
c('MoLing',     'j', .34, .34, 1.54, INK, .34, .94, 1.14, 'tl', .66)
c('GuiPo',      'h', .36, .46, 1.56, JADE, .34, .94, 1.14, 'br', .66)
c('HunGui',     'i', .62, .46, 1.56, DARK, .36, .90, 1.16, 'br', .68)
c('ZuoSuiJing', 'g', .34, .30, 1.56, GOLD, .32, .96, 1.12, 'tl', .66)

# ---- Ancient 4（金紫浓调）----
c('GuiMoZhenYan',  'a', .62, .38, 1.58, GOLD, .40, .96, 1.18, 'tl', .68)
c('XianGuZhiZhao', 'd', .50, .50, 1.62, GOLD, .40, .94, 1.18, 'br', .70)
c('MoYu',          'e', .50, .46, 1.60, DARK, .42, .90, 1.20, 'tl', .70)
c('WanJieLaiChao', 'f', .50, .40, 1.62, GOLD, .40, .96, 1.18, 'tr', .70)

# ============================================================
# 遗物 2
# ============================================================
RELICS = {
    'GuiMo':      ('d', .44, .44, 2.30, INK,  .42, .96, 1.16),
    'JinSiGuiMo': ('d', .66, .40, 2.30, GOLD, .34, 1.08, 1.18),
}

# ============================================================
# 能力 36（全部新能力 + 保留的临时力量/敏捷 + 追踪器）
# ============================================================
POWERS = {
    'ZuoSuiPower':               ('d', .55, .42, 2.20, INK,  .44, .94, 1.18),
    'ZuoSuiPlayedThisTurnPower': ('d', .62, .30, 2.30, INK,  .40, .96, 1.14),
    'ZuoSuiZhangPower':          ('g', .34, .34, 2.30, INK,  .40, .94, 1.16),
    'ZuoSuiJingPower':           ('g', .60, .42, 2.30, GOLD, .36, .96, 1.14),
    'DianZengPower':             ('f', .34, .44, 2.30, GOLD, .32, .98, 1.10),
    'XianZengPower':             ('h', .60, .34, 2.30, GOLD, .32, .98, 1.10),
    'MianZengPower':             ('i', .32, .50, 2.30, GOLD, .32, .98, 1.10),
    'WanJieLinPower':            ('a', .50, .46, 2.25, GOLD, .42, .94, 1.18),
    'GuiLuoPower':               ('d', .58, .40, 2.35, INK,  .42, .92, 1.18),
    'GuiMoZhenYanPower':         ('a', .62, .38, 2.30, GOLD, .44, .94, 1.20),
    'GuiMoXinPower':             ('g', .40, .40, 2.30, JADE, .36, .96, 1.14),
    'GuiPoPower':                ('h', .36, .46, 2.35, JADE, .38, .94, 1.16),
    'XinMoPower':                ('g', .34, .50, 2.30, JADE, .34, .96, 1.12),
    'GuiYingPower':              ('f', .60, .40, 2.35, DARK, .36, .92, 1.16),
    'CanYingPower':              ('g', .58, .52, 2.35, DARK, .38, .90, 1.18),
    'MoShenPower':               ('e', .50, .30, 2.30, GOLD, .38, .96, 1.16),
    'WanJieLaiChaoPower':        ('f', .50, .40, 2.30, GOLD, .44, .94, 1.20),
    'MoYinPower':                ('j', .32, .36, 2.30, INK,  .36, .94, 1.14),
    'MoLingPower':               ('j', .34, .34, 2.35, INK,  .38, .92, 1.16),
    'HunGuiPower':               ('i', .62, .46, 2.35, DARK, .40, .90, 1.18),
    'GuiXiPower':                ('e', .36, .44, 2.30, JADE, .34, .96, 1.12),
    'LingHunPower':              ('h', .34, .50, 2.35, DARK, .38, .92, 1.16),
    'XuShiJiePower':             ('i', .60, .48, 2.30, PALE, .30, .98, 1.02),
    'XuKongJiePower':            ('f', .62, .34, 2.35, DARK, .38, .92, 1.16),
    'LingXuPower':               ('h', .62, .30, 2.30, GOLD, .34, .96, 1.12),
    'MoXinPower':                ('j', .58, .30, 2.30, JADE, .34, .96, 1.12),
    'MoShengPower':              ('i', .36, .44, 2.30, PALE, .30, .98, 1.02),
    'GuiMoJiaPower':             ('d', .40, .30, 2.30, DARK, .38, .94, 1.16),
    'LingLiPower':               ('a', .44, .36, 2.25, BLOOD, .32, .98, 1.12),
    'MoYiNextCardPower':         ('i', .30, .42, 2.30, PALE, .30, .98, 1.00),
    'HuiMoLianPower':            ('j', .56, .52, 2.35, DARK, .40, .90, 1.18),
    'WanJieRuLinCombatTrackerPower': ('a', .50, .50, 2.60, INK, .30, .98, 1.06),
    'WanJieTempStrengthPower':   ('a', .70, .30, 2.30, BLOOD, .34, 1.06, 1.16),
    'WanJieTempDexterityPower':  ('b', .34, .34, 2.30, JADE, .30, 1.08, 1.10),
    'GhostQiGainedThisTurnPower': ('e', .40, .44, 2.35, JADE, .34, .96, 1.12),
    'CardsPlayedThisTurnPower':  ('c', .50, .40, 2.60, INK, .30, .98, 1.06),
}

# ============================================================
n = 0
for cls, (src, fx, fy, zoom, rgb, ts, bright, sat, corner, vig) in CARDS.items():
    img = make(src, fx, fy, zoom, rgb, ts, bright, sat, (CARD_W, CARD_H), corner, vig)
    save(img, os.path.join(OUT, 'cards', cls + '.png'))
    n += 1
print('cards:', n)

n = 0
for cls, (src, fx, fy, zoom, rgb, ts, bright, sat) in RELICS.items():
    img = make(src, fx, fy, zoom, rgb, ts, bright, sat, (SIZE, SIZE), 'tl', .62)
    img = glow(img, rgb, radius=18, strength=.40)
    save(img, os.path.join(OUT, 'relics', cls + '.png'))
    n += 1
print('relics:', n)

n = 0
for cls, cfg in POWERS.items():
    src, fx, fy, zoom, rgb, ts, bright, sat = cfg[:8]
    corner = cfg[8] if len(cfg) > 8 else 'tl'
    vig = cfg[9] if len(cfg) > 9 else .66
    img = make(src, fx, fy, zoom, rgb, ts, bright, sat, (SIZE, SIZE), corner, vig)
    img = glow(img, rgb, radius=20, strength=.42)
    save(img, os.path.join(OUT, 'powers', cls + '.png'))
    n += 1
print('powers:', n)
print('done')
