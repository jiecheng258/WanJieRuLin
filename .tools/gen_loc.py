# -*- coding: utf-8 -*-
"""gen_loc.py (v0.5) — 只产出「点·线·面」新设计的本地化。

产物: localization/{zhs,eng}/{cards,powers,characters,static_hover_tips,card_keywords}.json
真相源: card_loc_v05.py (由 _v05_loc.py 从生成器的 doc 自动产出)
"""
import io, os, re, json, sys

ROOT = r'C:/Users/wangx/Documents/Default Project\WanJieRuLin\WanJieRuLin\localization'
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import card_loc_v05 as CL

PFX = 'WAN_JIE_RU_LIN_'


def snake(cls):
    return re.sub(r'(?<!^)(?=[A-Z])', '_', cls).upper()


def write(path, data):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    io.open(path, 'w', encoding='utf-8', newline='\n').write(
        json.dumps(dict(sorted(data.items())), ensure_ascii=False, indent=2))
    print('  wrote %-44s %d keys' % (os.path.relpath(path, ROOT), len(data)))


POWERS = {
    'LuanDianPower': ('斑驳乱点',
        '[gold]斑驳乱点[/gold]：本回合已打出 {Amount} 张「点」牌。'
        '超过 3 张后，每多 1 张依次：力量 −2 / 敏捷 −2 / 虚弱 2 / 易伤 2 / 塞入 1 张诅咒。回合结束清空。',
        'Mottled Dots',
        '[gold]Mottled Dots[/gold]: Point cards played this turn. '
        'Past 3, each extra one adds a penalty: -2 Str / -2 Dex / 2 Weak / 2 Vulnerable / a curse. Clears at end of turn.'),
    'QianJunPower': ('千钧一线',
        '[gold]千钧一线[/gold]：本回合已打出 {Amount} 张「线」牌。每张线牌额外 +1 临时力量/敏捷并抽 1 张。',
        'A Thread of Life',
        '[gold]A Thread of Life[/gold]: Line cards played this turn. Each Line card also gives +1 Temp Str/Dex and draws 1.'),
    'WanJieTempStrengthPower': ('临时力量',
        '本回合力量 +{Amount}（回合结束消失）。',
        'Temporary Strength',
        'Gain +{Amount} Strength this turn (removed at end of turn).'),
    'WanJieTempDexterityPower': ('临时敏捷',
        '本回合敏捷 +{Amount}（回合结束消失）。',
        'Temporary Dexterity',
        'Gain +{Amount} Dexterity this turn (removed at end of turn).'),
    'RunBiPower': ('润笔',
        '每回合你打出的第一张「点」牌额外抽 {Amount} 张牌。',
        'Inked Brush',
        'The first Point card you play each turn draws {Amount} extra cards.'),
    'LiTouPower': ('力透纸背',
        '每回合开始时获得 {Force} 点临时力量。',
        'Force Through',
        'At the start of each turn, gain {Force} Temporary Strength.'),
    'PoTianPower': ('泼天',
        '你的「面」牌伤害额外 +{PerStep}。',
        'Ink Deluge',
        'Your Face cards deal +{PerStep} extra damage.'),
    'YiQiHeChengPower': ('一气呵成',
        '你打出「面」牌时，本回合获得 {Amount} 点临时力量。',
        'One Stroke',
        'When you play a Face card, gain {Amount} Temporary Strength this turn.'),
    'ZhangChiYouDuPower': ('张弛有度',
        '你打出「点」牌时，抽 1 张牌（每回合上限 {Cap} 次）。',
        'Ebb and Flow',
        'When you play a Point card, draw 1 card (up to {Cap} times per turn).'),
    'FaceReturnResetPower': ('去年今日此门中',
        '面牌回流记录（每场战斗重置）。',
        'Same Gate, Next Year',
        'Face card return tracking (resets each combat).'),

    'BiFengPower': ('笔锋',
        '[gold]笔锋[/gold]：**本回合最多触发 3 次**，每次获得 [blue]1[/blue] 点能量。'
        '（回合结束清零）',
        'Edge',
        '[gold]Edge[/gold]: **up to 3 times per turn**, gain [blue]1[/blue] Energy each. '
        'Resets at end of turn.'),
    'LiDaoPower': ('力道',
        '[gold]力道[/gold]：本回合你打出的牌伤害与格挡 +{Amount}。',
        'Force',
        '[gold]Force[/gold]: your cards this turn deal +{Amount} damage and gain +{Amount} Block.'),
    'MoYunPower': ('墨韵',
        '[gold]墨韵[/gold]：跨回合累积。每满 5 层，你的「点」「线」牌效果 −1。',
        'Ink',
        '[gold]Ink[/gold]: accumulates across turns. Every 5 stacks, your Point/Line cards are 1 weaker.'),
    'RunBiPower': ('润笔',
        '每当你打出一张「点」牌，获得 1 点能量（每回合上限 {Cap} 次）。',
        'Inked Brush',
        'Whenever you play a Point card, gain 1 Energy (up to {Cap} times per turn).'),
    'LiTouPower': ('力透纸背',
        '每回合开始时获得 {Force} 点[gold]力道[/gold]。',
        'Force Through',
        'At the start of each turn, gain {Force} [gold]Force[/gold].'),
    'PoTianPower': ('泼天',
        '[gold]墨韵[/gold]不再削弱「点 / 线」牌；改为每满 5 层，你的「面」牌伤害 +{PerStep}。',
        'Ink Deluge',
        '[gold]Ink[/gold] no longer weakens Point/Line cards; every 5 stacks gives your Face cards +{PerStep} damage.'),
    'ZongHengPower': ('纵横',
        '本回合内，你每打出一张「线」牌，获得 {Amount} 点[gold]力道[/gold]。',
        'Sweeping Stroke',
        'This turn, whenever you play a Line card, gain {Amount} [gold]Force[/gold].'),
    'WuShiWuZhongPower': ('无始无终',
        '每回合最多触发 {Cap} 次：你打出「点」牌时，抽 1 张牌。',
        'Endless',
        'Up to {Cap} times per turn: when you play a Point card, draw 1 card.'),
    'HuiFengTracePower': ('回锋标记',
        '本回合已打出过「回锋」。',
        'Echo Mark',
        'You played Echo Edge this turn.'),
}


RELIC_ZH = {
    PFX + 'RELIC_WEI_WANG_CHENG_DE_ZI_HUA_XIANG.title': '未完成的自画像',
    PFX + 'RELIC_WEI_WANG_CHENG_DE_ZI_HUA_XIANG.description':
        '每回合开始时获得 1 点[gold]笔锋[/gold]。\n每当你打出一张「面」牌，获得 1 点[gold]墨韵[/gold]。',
}
RELIC_EN = {
    PFX + 'RELIC_WEI_WANG_CHENG_DE_ZI_HUA_XIANG.title': 'Unfinished Self-Portrait',
    PFX + 'RELIC_WEI_WANG_CHENG_DE_ZI_HUA_XIANG.description':
        'At the start of each turn, gain 1 [gold]Edge[/gold].\nWhenever you play a Face card, gain 1 [gold]Ink[/gold].',
}

CHARACTER_ZH = {
    PFX + 'CHARACTER_WAN_JIE_RU_LIN_CHARACTER.title': '万界如林',
    PFX + 'CHARACTER_WAN_JIE_RU_LIN_CHARACTER.titleObject': '万界如林',
    PFX + 'CHARACTER_WAN_JIE_RU_LIN_CHARACTER.description':
        '「林」的身份一直是一个谜。\n有关于她的故事有很多，或许会有真的。\n她说：我来自于……',
}
CHARACTER_EN = {
    PFX + 'CHARACTER_WAN_JIE_RU_LIN_CHARACTER.title': 'WanJieRuLin',
    PFX + 'CHARACTER_WAN_JIE_RU_LIN_CHARACTER.titleObject': 'WanJieRuLin',
    PFX + 'CHARACTER_WAN_JIE_RU_LIN_CHARACTER.description':
        'Nobody knows who "Lin" really is.\nThere are many stories about her. Some might be true.\nShe said: I come from...',
}

TIPS_ZH = {
    PFX + 'POINT_TITLE': '点',
    PFX + 'POINT_DESC': '费用低、数值偏低的牌。产出[gold]笔锋[/gold]，让你打出的下一张牌更便宜。',
    PFX + 'LINE_TITLE': '线',
    PFX + 'LINE_DESC': '费用与数值都均衡的牌。产出[gold]力道[/gold]，增强你本回合打出的牌。',
    PFX + 'FACE_TITLE': '面',
    PFX + 'FACE_DESC': '费用高、数值高的牌。产出[gold]墨韵[/gold]（跨回合累积）。',
}
TIPS_EN = {
    PFX + 'POINT_TITLE': 'Point',
    PFX + 'POINT_DESC': 'Cheap, low-value cards. Produce [gold]Edge[/gold], making your next card cheaper.',
    PFX + 'LINE_TITLE': 'Line',
    PFX + 'LINE_DESC': 'Balanced cards. Produce [gold]Force[/gold], boosting the cards you play this turn.',
    PFX + 'FACE_TITLE': 'Face',
    PFX + 'FACE_DESC': 'Expensive, high-value cards. Produce [gold]Ink[/gold] (accumulates across turns).',
}

KW_ZH = {'EXHAUST.description':
         '打出后进入[gold]消耗堆[/gold]，本场战斗[gold]不会再回到抽牌堆[/gold]。'}
KW_EN = {'EXHAUST.description':
         'After being played, this card goes to your [gold]Exhaust pile[/gold] and never returns to your draw pile this combat.'}

for lang in ('zhs', 'eng'):
    zh = (lang == 'zhs')
    base = os.path.join(ROOT, lang)
    print('[%s]' % lang)

    cards = {}
    for cls, text in (CL.ZH if zh else CL.EN).items():
        k = PFX + 'CARD_' + snake(cls) + '.'
        cards[k + 'title'] = (CL.TITLES_ZH if zh else CL.TITLES_EN).get(cls, cls)
        cards[k + 'description'] = text
        cards[k + 'smartDescription'] = text
    write(os.path.join(base, 'cards.json'), cards)

    powers = {}
    for cls, v in POWERS.items():
        k = PFX + 'POWER_' + snake(cls) + '.'
        powers[k + 'title'] = v[0] if zh else v[2]
        powers[k + 'description'] = v[1] if zh else v[3]
        powers[k + 'smartDescription'] = v[1] if zh else v[3]
    write(os.path.join(base, 'powers.json'), powers)

    write(os.path.join(base, 'relics.json'), RELIC_ZH if zh else RELIC_EN)
    write(os.path.join(base, 'characters.json'), CHARACTER_ZH if zh else CHARACTER_EN)
    write(os.path.join(base, 'static_hover_tips.json'), TIPS_ZH if zh else TIPS_EN)
    write(os.path.join(base, 'card_keywords.json'), KW_ZH if zh else KW_EN)

print('done')
