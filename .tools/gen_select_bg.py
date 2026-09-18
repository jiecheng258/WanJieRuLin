"""生成「万界如林」角色选择界面背景图。

源图：C:/Users/wangx/Desktop/人物背景.jpg (2560x1200，暖色动画风)

角色选择背景面板在原版 UI 里约为 1280x1078（竖长条，见
WanJieRuLin_character_select_bg.tscn 里 Background 节点的尺寸）。
但背景是铺满整个选择区域的，实际要做的是「宽屏底图」：
按 1920x1080 出图，Godot 侧再用 stretch_mode 拉伸。

这里做两件事：
1. 从 2560x1200 裁一块视觉重心合适的区域（人物在右侧，保留她 + 桌面）。
2. 右侧叠一层向紫黑过渡的渐变，让左边的角色立绘和文字能压住背景，
   同时保持「鬼墨」主题色调。
"""
import os

from PIL import Image, ImageDraw, ImageFilter

SRC = r'C:/Users/wangx/Desktop/人物背景.jpg'
OUT_DIR = r'C:/Users/wangx/Documents/Default Project/WanJieRuLin/WanJieRuLin/images/characters'
OUT_NAME = 'WanJieRuLin_character_select_bg.png'

# 出图尺寸：游戏 UI 走 1080p 基准。
W, H = 1920, 1080

# 主题色（与卡框、能量轮廓一致）：墨黑幽紫
INK = (22, 16, 37)
INK_MID = (42, 28, 66)
PLUM = (77, 50, 111)


def main():
    src = Image.open(SRC).convert('RGB')
    sw, sh = src.size
    print('source %dx%d' % (sw, sh))

    # --- 1. 裁切 -----------------------------------------------------------
    # 源图 2560x1200 比例 2.13；目标 1920x1080 比例 1.78。
    # 所以按高度对齐、横向裁掉两侧。人物偏右，保留中右段。
    target_ratio = W / H
    crop_h = sh
    crop_w = int(round(crop_h * target_ratio))   # 1200*1.78 = 2136
    if crop_w > sw:
        crop_w = sw
        crop_h = int(round(crop_w / target_ratio))

    # 横向取景：让人物落在画面右 1/3，左侧留出给立绘与文字。
    left = int((sw - crop_w) * 0.62)
    top = int((sh - crop_h) * 0.5)
    box = (left, top, left + crop_w, top + crop_h)
    print('crop box', box)
    img = src.crop(box).resize((W, H), Image.LANCZOS)

    # --- 2. 轻度压暗即可，尽量保住原图的暖调 --------------------------------
    img = Image.eval(img, lambda v: int(v * 0.88))

    # --- 3. 左侧渐深：文字与立绘区要压得住，但右侧几乎不动 -------------------
    shade = Image.new('L', (W, H), 0)
    d = ImageDraw.Draw(shade)
    for x in range(W):
        t = x / (W - 1)
        if t < 0.38:
            # 最左边很暗，到 38% 处降到中等
            a = int(190 - 110 * (t / 0.38))
        else:
            # 再往右快速衰减到 0，露出原图
            a = int(80 * max(0.0, 1 - (t - 0.38) / 0.42) ** 1.8)
        d.line([(x, 0), (x, H)], fill=max(0, min(255, a)))

    ink_layer = Image.new('RGB', (W, H), INK)
    img = Image.composite(ink_layer, img, shade)

    # --- 4. 只在四边加很薄的紫墨色晕，中间保持通透 ---------------------------
    grad = Image.new('L', (W, H), 0)
    dg = ImageDraw.Draw(grad)
    for y in range(H):
        t = y / (H - 1)
        # 顶部和底部各压一点，中间基本不压
        edge = min(t, 1 - t)
        a = int(95 * max(0.0, 1 - edge / 0.30) ** 1.6)
        dg.line([(0, y), (W, y)], fill=a)

    plum_layer = Image.new('RGB', (W, H), INK_MID)
    img = Image.composite(plum_layer, img, grad)

    # --- 5. 极轻散焦：只够让背景退后，不糊掉主体 -----------------------------
    img = img.filter(ImageFilter.GaussianBlur(radius=1.6))

    # --- 6. 暗角：只收边角 ---------------------------------------------------
    vig = Image.new('L', (W, H), 0)
    dv = ImageDraw.Draw(vig)
    cx, cy = W / 2, H / 2
    max_d = (cx ** 2 + cy ** 2) ** 0.5
    step = 6
    for y in range(0, H, step):
        for x in range(0, W, step):
            r = ((x - cx) ** 2 + (y - cy) ** 2) ** 0.5 / max_d
            a = int(115 * max(0.0, (r - 0.72) / 0.28) ** 1.5)
            dv.rectangle([x, y, x + step, y + step], fill=a)
    vig = vig.filter(ImageFilter.GaussianBlur(radius=40))
    img = Image.composite(Image.new('RGB', (W, H), (10, 7, 17)), img, vig)

    out = os.path.join(OUT_DIR, OUT_NAME)
    img.save(out, 'PNG', optimize=True)
    print('wrote %s  (%dx%d, %.1f MB)'
          % (out, W, H, os.path.getsize(out) / 1024 / 1024))

    # 审查用缩略图
    img.resize((640, 360)).save(
        r'C:/Users/wangx/WorkBuddy/2026-09-15-20-17-56/.tools/preview_select_bg.jpg',
        quality=88)
    print('preview written')


if __name__ == '__main__':
    main()
