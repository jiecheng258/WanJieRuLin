# -*- coding: utf-8 -*-
"""
从用户提供的 4 张图生成 WanJieRuLin 的全部美术资源：
  - 卡图  images/cards/{Class}.png          （52 张）
  - 遗物  images/relics/{Class}.png          （6 个）
  - 能力  images/powers/{Class}.png          （23 个）

策略：以 4 张原图为"画源"，按卡牌的类型/稀有度/元素主题做不同的
裁剪区、色调、明暗与叠层处理，让每张卡都有独立且贴合主题的视觉。
"""
import os
from PIL import Image, ImageDraw, ImageFilter, ImageEnhance, ImageChops

SRC = {
    'a': r'C:\Users\wangx\Desktop\人物背景.jpg',      # 4.2MB 横幅，最细腻
    'b': r'C:\Users\wangx\Desktop\卡牌背景1.jpg',     # 蓝天蝴蝶，明亮
    'c': r'C:\Users\wangx\Desktop\卡牌背景2.jpg',     # 米黄，柔光
    'd': r'C:\Users\wangx\Desktop\卡牌背景3.jpg',     # 暗色，符纸，神秘
}
OUT = r'C:\Users\wangx\Documents\Default Project\WanJieRuLin\WanJieRuLin\images'

CARD_W, CARD_H = 500, 380      # 卡图肖像比例
RELIC = 128
POWER = 128

# ------------------------------------------------------------------ 调色记忆
INK = (0.70, 0.52, 0.98)       # 幽紫（角色主色）
GOLD = (0.98, 0.80, 0.42)
BLOOD = (0.86, 0.28, 0.34)
JADE = (0.42, 0.84, 0.72)
PALE = (0.86, 0.84, 0.92)
DARK = (0.10, 0.08, 0.16)


def tint(img, rgb, strength=0.35):
    """把图像整体偏向某个颜色。"""
    r, g, b = [int(c * 255) for c in rgb]
    layer = Image.new('RGB', img.size, (r, g, b))
    return Image.blend(img, layer, strength)


def crop_to(img, w, h, fx=0.5, fy=0.42, zoom=1.0):
    """按比例裁剪到目标宽高比，fx/fy 决定取景中心，zoom>1 放大。"""
    iw, ih = img.size
    target = w / h
    cur = iw / ih
    if cur > target:
        nh = ih; nw = int(ih * target)
    else:
        nw = iw; nh = int(iw / target)
    nw = int(nw / zoom); nh = int(nh / zoom)
    x = int((iw - nw) * fx); y = int((ih - nh) * fy)
    x = max(0, min(x, iw - nw)); y = max(0, min(y, ih - nh))
    return img.crop((x, y, x + nw, y + nh)).resize((w, h), Image.LANCZOS)


def vignette(img, power=0.55):
    """四角压暗，让主体更聚焦。"""
    w, h = img.size
    mask = Image.new('L', (w, h), 0)
    d = ImageDraw.Draw(mask)
    d.ellipse((-w * 0.25, -h * 0.30, w * 1.25, h * 1.30), fill=255)
    mask = mask.filter(ImageFilter.GaussianBlur(min(w, h) * 0.18))
    dark = ImageEnhance.Brightness(img).enhance(1 - power)
    return Image.composite(img, dark, mask)


def grain(img, amount=6):
    """轻微噪点，避免生成的图过于平滑而不像手绘。"""
    w, h = img.size
    noise = Image.effect_noise((w, h), amount).convert('L')
    noise = noise.filter(ImageFilter.GaussianBlur(0.6))
    return ImageChops.overlay(img, Image.merge('RGB', (noise, noise, noise)))


def glow(img, rgb, radius=40, strength=0.30):
    """边缘辉光。"""
    w, h = img.size
    r, g, b = [int(c * 255) for c in rgb]
    halo = Image.new('RGB', (w, h), (r, g, b))
    mask = Image.new('L', (w, h), 0)
    d = ImageDraw.Draw(mask)
    m = int(min(w, h) * 0.10)
    d.rectangle((m, m, w - m, h - m), outline=255, width=max(4, m // 2))
    mask = mask.filter(ImageFilter.GaussianBlur(radius))
    return Image.composite(Image.blend(img, halo, strength), img, mask)


def ink_wash(img, rgb, strength=0.18, corner='tl'):
    """在角上叠一层墨色渐变，增加"画"的味道。"""
    w, h = img.size
    r, g, b = [int(c * 255) for c in rgb]
    grad = Image.new('L', (w, h), 0)
    d = ImageDraw.Draw(grad)
    steps = 60
    for i in range(steps):
        t = i / steps
        alpha = int(255 * (1 - t) ** 2)
        if corner == 'tl':
            d.polygon([(0, 0), (w * t, 0), (0, h * t)], fill=alpha)
        elif corner == 'br':
            d.polygon([(w, h), (w - w * t, h), (w, h - h * t)], fill=alpha)
        else:
            d.polygon([(w, 0), (w - w * t, 0), (w, h * t)], fill=alpha)
    grad = grad.filter(ImageFilter.GaussianBlur(18))
    layer = Image.new('RGB', (w, h), (r, g, b))
    return Image.composite(layer, img, grad.point(lambda v: int(v * strength)))


# ------------------------------------------------------------------ 卡牌主题表
# (源图, 取景fx, 取景fy, 缩放, 主色调, 强度, 明度, 饱和)
CARDS = {}


def reg(cls, src, fx, fy, zoom, rgb, ts, bright, sat, corner='tl', vig=0.55):
    CARDS[cls] = dict(src=src, fx=fx, fy=fy, zoom=zoom, rgb=rgb, ts=ts,
                      bright=bright, sat=sat, corner=corner, vig=vig)


# ---- 基础（3）----
reg('DaJi',            'a', 0.72, 0.34, 1.30, INK,   0.16, 1.02, 1.05)
reg('FangYu',          'c', 0.50, 0.38, 1.25, PALE,  0.16, 1.06, 0.92, 'br')
reg('Hui',             'd', 0.62, 0.40, 1.35, INK,   0.34, 0.98, 1.12)

# ---- 普通（12）----
reg('FengBi',          'c', 0.34, 0.40, 1.45, PALE,  0.22, 1.04, 0.90)
reg('QinShi',          'd', 0.30, 0.50, 1.55, BLOOD, 0.34, 0.94, 1.18, 'br', 0.62)
reg('MoDian',          'd', 0.72, 0.46, 1.40, INK,   0.36, 0.96, 1.10)
reg('TuiGui',          'd', 0.46, 0.58, 1.70, JADE,  0.38, 0.92, 1.20, 'br', 0.64)
reg('GuangMingQieGe',  'b', 0.34, 0.36, 1.50, GOLD,  0.30, 1.08, 1.08)
reg('BiMoQingXie',     'a', 0.42, 0.50, 1.15, INK,   0.30, 0.98, 1.10, 'tr')
reg('ShenShiDuoShi',   'c', 0.66, 0.34, 1.40, GOLD,  0.24, 1.06, 1.02)
reg('ChiXuQinRao',     'd', 0.36, 0.44, 1.48, BLOOD, 0.32, 0.95, 1.14)
reg('XiaoHuo',         'b', 0.62, 0.32, 1.42, JADE,  0.26, 1.04, 1.00, 'tr')
reg('LueDuo',          'a', 0.78, 0.46, 1.22, GOLD,  0.28, 1.00, 1.08)
reg('DaMengYiChang',   'c', 0.44, 0.30, 1.30, PALE,  0.20, 1.08, 0.94)
reg('DianXian',        'b', 0.50, 0.42, 1.28, INK,   0.30, 1.02, 1.06, 'br')

# ---- 罕见（20）----
reg('YinYangGeHunXiao','a', 0.50, 0.34, 1.45, INK,   0.36, 0.96, 1.14)
reg('GuiYingSenSen',   'd', 0.54, 0.52, 1.62, INK,   0.44, 0.88, 1.22, 'br', 0.70)
reg('TunShi',          'd', 0.24, 0.56, 1.75, BLOOD, 0.42, 0.88, 1.24, 'tl', 0.70)
reg('HuaShen',         'a', 0.62, 0.28, 1.35, JADE,  0.30, 1.06, 1.10, 'tr', 0.50)
reg('YinSen',          'd', 0.40, 0.60, 1.80, INK,   0.46, 0.86, 1.24, 'br', 0.72)
reg('HuiHuaFenGe',     'a', 0.30, 0.44, 1.38, BLOOD, 0.32, 0.98, 1.12)
reg('ShengWenZi',      'd', 0.68, 0.56, 1.66, GOLD,  0.36, 0.96, 1.16, 'tr', 0.64)
reg('XiaBi',           'a', 0.86, 0.36, 1.30, INK,   0.28, 1.00, 1.08)
reg('YouYouDianLong',  'b', 0.72, 0.40, 1.36, JADE,  0.30, 1.04, 1.10, 'br')
reg('YanMoXiaBi',      'a', 0.18, 0.56, 1.52, BLOOD, 0.34, 0.96, 1.14, 'tl')
reg('PiaoMiaoJianJue', 'b', 0.24, 0.30, 1.44, PALE,  0.22, 1.08, 1.00, 'tr', 0.48)
reg('CengCengQinShi',  'c', 0.28, 0.52, 1.52, JADE,  0.28, 1.00, 1.06, 'bl')
reg('WoRuoWeiGui',     'd', 0.58, 0.60, 1.72, INK,   0.46, 0.88, 1.22, 'br', 0.72)
reg('WoRuoWeiShen',    'b', 0.38, 0.26, 1.40, GOLD,  0.26, 1.10, 1.04, 'tl', 0.46)
reg('ZanBiFengMang',   'c', 0.74, 0.56, 1.46, PALE,  0.24, 1.02, 0.92, 'br')
reg('YaZhiYuWang',     'd', 0.32, 0.38, 1.58, INK,   0.40, 0.92, 1.16)
reg('ShenShengBaoZou', 'a', 0.56, 0.62, 1.60, BLOOD, 0.38, 0.96, 1.20, 'tr', 0.62)
reg('DuZiShengJi',     'c', 0.60, 0.44, 1.38, JADE,  0.26, 1.04, 1.04)
reg('DianLongHuFa',    'b', 0.84, 0.52, 1.34, JADE,  0.30, 1.02, 1.10, 'br')
reg('HeiAnBiZhang',    'd', 0.76, 0.30, 1.54, INK,   0.48, 0.84, 1.20, 'tl', 0.74)

# ---- 稀有（14）----
reg('DianLongXingTai', 'b', 0.60, 0.24, 1.30, JADE,  0.32, 1.06, 1.14, 'tr', 0.50)
reg('LiGuiFuSu',       'd', 0.44, 0.34, 1.64, INK,   0.50, 0.86, 1.24, 'br', 0.74)
reg('GuangMingYuYan',  'b', 0.30, 0.22, 1.34, GOLD,  0.30, 1.10, 1.08, 'tl', 0.46)
reg('GuiYu',           'd', 0.50, 0.66, 1.90, INK,   0.54, 0.84, 1.26, 'br', 0.78)
reg('YangGuangPuZhao', 'b', 0.50, 0.18, 1.26, GOLD,  0.28, 1.14, 1.06, 'tl', 0.42)
reg('YouRanGuiHuo',    'd', 0.62, 0.48, 1.58, JADE,  0.40, 0.94, 1.20, 'tr', 0.64)
reg('XianMian',        'a', 0.36, 0.62, 1.48, INK,   0.34, 0.98, 1.12)
reg('MangXinJian',     'a', 0.22, 0.40, 1.56, BLOOD, 0.36, 0.96, 1.16, 'tl', 0.66)
reg('AoMan',           'a', 0.92, 0.30, 1.44, GOLD,  0.30, 1.02, 1.12, 'tr')
reg('ShiTong',         'd', 0.66, 0.34, 1.70, BLOOD, 0.44, 0.90, 1.22, 'br', 0.70)
reg('PingXingShiJie',  'c', 0.38, 0.26, 1.32, PALE,  0.24, 1.08, 1.00, 'tr', 0.48)
reg('AoJiao',          'c', 0.72, 0.36, 1.28, GOLD,  0.24, 1.08, 1.06)
reg('Jian',            'b', 0.80, 0.30, 1.40, PALE,  0.26, 1.06, 1.08, 'tr', 0.50)
reg('WoBuWanLe',       'c', 0.48, 0.52, 1.24, INK,   0.30, 1.02, 1.08, 'bl')

# ---- 先古 / 事件 / 衍生物（3）----
reg('MoRanJiangShan',  'd', 0.50, 0.50, 1.60, INK,   0.50, 0.90, 1.20, 'tl', 0.68)
reg('GuiQiSenSen',     'd', 0.26, 0.28, 1.66, JADE,  0.48, 0.88, 1.22, 'br', 0.72)
reg('YinSenSen',       'd', 0.56, 0.72, 1.86, INK,   0.52, 0.84, 1.24, 'tr', 0.76)

# ---- 遗物（6）----
RELICS = {
    'GuiMo':           ('d', 0.44, 0.44, 2.30, INK,   0.42, 0.96, 1.16),
    'JinSiGuiMo':      ('d', 0.66, 0.40, 2.30, GOLD,  0.34, 1.08, 1.18),
    'YiBaXiaoJian':    ('b', 0.26, 0.28, 2.60, PALE,  0.24, 1.10, 1.06),
    'TianShiSan':      ('c', 0.40, 0.30, 2.40, GOLD,  0.28, 1.08, 1.04),
    'BeiLeiMao':       ('a', 0.78, 0.20, 2.70, INK,   0.26, 1.04, 1.10),
    '_default':        ('c', 0.50, 0.40, 2.30, INK,   0.30, 1.02, 1.06),
}

# ---- 能力（23）----
POWERS = {
    'EnergyToGhostQiPower':  ('d', 0.40, 0.34, 2.20, INK,   0.44, 0.98, 1.18),
    'GhostQiToEnergyPower':  ('b', 0.56, 0.30, 2.20, GOLD,  0.32, 1.10, 1.12),
    'GhostQiNextTurnPower':  ('d', 0.60, 0.44, 2.30, INK,   0.40, 0.98, 1.14),
    'GuiYingSenSenPower':    ('d', 0.30, 0.56, 2.50, INK,   0.48, 0.90, 1.22),
    'TunShiPower':           ('d', 0.22, 0.62, 2.60, BLOOD, 0.44, 0.90, 1.22),
    'HuaShenPower':          ('a', 0.64, 0.26, 2.30, JADE,  0.30, 1.10, 1.12),
    'YinYangGeHunXiaoPower': ('a', 0.46, 0.36, 2.40, INK,   0.38, 1.00, 1.16),
    'BideDefensivelyPower':  ('c', 0.72, 0.54, 2.30, PALE,  0.24, 1.04, 0.94),
    'DivineRampagePower':    ('a', 0.54, 0.58, 2.45, BLOOD, 0.40, 0.98, 1.20),
    'SoloLevelUpPower':      ('c', 0.58, 0.38, 2.20, JADE,  0.26, 1.06, 1.06),
    'DianLongFormPower':     ('b', 0.62, 0.24, 2.20, JADE,  0.32, 1.08, 1.16),
    'GuiYuPower':            ('d', 0.52, 0.66, 2.70, INK,   0.54, 0.86, 1.26),
    'ShiTongPower':          ('d', 0.68, 0.32, 2.50, BLOOD, 0.46, 0.92, 1.22),
    'AoManPower':            ('a', 0.92, 0.28, 2.40, GOLD,  0.30, 1.04, 1.14),
    'SwordStormPower':       ('b', 0.78, 0.30, 2.40, PALE,  0.26, 1.08, 1.08),
    'WoBuWanLePower':        ('c', 0.46, 0.50, 2.20, INK,   0.32, 1.02, 1.10),
    'GuangMingYuYanPower':   ('b', 0.28, 0.20, 2.20, GOLD,  0.30, 1.12, 1.10),
    'LastTurnCardsPower':    ('c', 0.36, 0.28, 2.20, PALE,  0.24, 1.06, 1.00),
    'JinJianPower':          ('d', 0.62, 0.38, 2.40, GOLD,  0.34, 1.10, 1.18),
    'MuJianPower':           ('c', 0.30, 0.44, 2.40, JADE,  0.30, 1.04, 1.08),
    'ShuiJianPower':         ('b', 0.24, 0.36, 2.40, PALE,  0.26, 1.10, 1.06),
    'HuoJianPower':          ('d', 0.36, 0.40, 2.40, BLOOD, 0.40, 1.00, 1.20),
    'TuJianPower':           ('c', 0.66, 0.56, 2.40, GOLD,  0.30, 1.02, 1.08),
    # 本模组的「临时力量 / 临时敏捷」包装能力（回合结束自动撤销）
    'WanJieTempStrengthPower':  ('a', 0.70, 0.30, 2.30, BLOOD, 0.34, 1.06, 1.16),
    'WanJieTempDexterityPower': ('b', 0.34, 0.34, 2.30, JADE,  0.30, 1.08, 1.10),
    '_default':              ('c', 0.50, 0.40, 2.20, INK,   0.30, 1.02, 1.06),
}

# ------------------------------------------------------------------ 执行
print('loading sources...')
SRCS = {}
for k, p in SRC.items():
    im = Image.open(p).convert('RGB')
    SRCS[k] = im
    print('  %s  %s' % (k, im.size))


def make(src, fx, fy, zoom, rgb, ts, bright, sat, size, corner, vig, extra_glow=True):
    img = crop_to(SRCS[src], size[0], size[1], fx, fy, zoom)
    img = tint(img, rgb, ts)
    img = ImageEnhance.Color(img).enhance(sat)
    img = ImageEnhance.Brightness(img).enhance(bright)
    img = ImageEnhance.Contrast(img).enhance(1.08)
    img = vignette(img, vig)
    img = ink_wash(img, rgb, 0.55, corner)
    if extra_glow:
        img = glow(img, rgb)
    img = grain(img, 5)
    return img


def save(img, path):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    img.save(path, 'PNG', optimize=True)


n = 0
for cls, c in CARDS.items():
    img = make(c['src'], c['fx'], c['fy'], c['zoom'], c['rgb'], c['ts'],
               c['bright'], c['sat'], (CARD_W, CARD_H), c['corner'], c['vig'])
    save(img, os.path.join(OUT, 'cards', cls + '.png'))
    n += 1
print('cards:', n)

n = 0
for cls, c in RELICS.items():
    if cls == '_default':
        continue
    img = make(c[0], c[1], c[2], c[3], c[4], c[5], c[6], c[7],
               (RELIC, RELIC), 'tl', 0.62, False)
    img = glow(img, c[4], radius=18, strength=0.40)
    save(img, os.path.join(OUT, 'relics', cls + '.png'))
    n += 1
print('relics:', n)

n = 0
for cls, c in POWERS.items():
    if cls == '_default':
        continue
    img = make(c[0], c[1], c[2], c[3], c[4], c[5], c[6], c[7],
               (POWER, POWER), 'tl', 0.66, False)
    img = glow(img, c[4], radius=20, strength=0.42)
    save(img, os.path.join(OUT, 'powers', cls + '.png'))
    n += 1
print('powers:', n)
print('done')
