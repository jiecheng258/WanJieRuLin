# -*- coding: utf-8 -*-
"""生成 WanJieRuLin 的全部本地化 JSON（zhs + eng）。"""
import json, os, io, re

ROOT = r'C:\Users\wangx\Documents\Default Project\WanJieRuLin\WanJieRuLin\localization'
CODE_ROOT = r'C:\Users\wangx\Documents\Default Project\WanJieRuLin\WanJieRuLinCode\Cards'

P = 'WAN_JIE_RU_LIN_CARD_'
C = 'WAN_JIE_RU_LIN_CARD_'


# ---------------------------------------------------------------- 鬼气费用解析
# 重要（bug 9）：RitsuLib 的次要资源费用 **不会**自动渲染进卡面描述，
# 自定义资源（鬼气）尤其看不见。所以「消耗 N 点鬼气」必须由本地化文案
# 自己写出来。这里直接从卡牌源码解析真实的 SetGhostQiCost / SetGhostQiCostX，
# 保证「牌面写的」永远等于「代码收的」，不会漂移。
def parse_ghost_qi_costs():
    """扫描代码目录，返回 {类名: 'X' 或 int}。"""
    costs = {}
    if not os.path.isdir(CODE_ROOT):
        return costs
    for f in sorted(os.listdir(CODE_ROOT)):
        if not f.endswith('.cs'):
            continue
        src = io.open(os.path.join(CODE_ROOT, f), encoding='utf-8', errors='replace').read()
        for m in re.finditer(r'class\s+(\w+)\s*:\s*WanJieRuLinCardModel', src):
            cls = m.group(1)
            start = m.end()
            nxt = src.find('class ', start)
            seg = src[start:nxt if nxt != -1 else len(src)]
            if 'SetGhostQiCostX' in seg:
                costs[cls] = 'X'
                continue
            mm = re.search(r'SetGhostQiCost\s*\(\s*([A-Za-z_0-9]+)\s*\)', seg)
            if not mm:
                continue
            arg = mm.group(1)
            if arg.isdigit():
                costs[cls] = int(arg)
            else:
                const = re.search(re.escape(arg) + r'\s*=\s*(\d+)', src)
                if const:
                    costs[cls] = int(const.group(1))
    return costs


GHOST_QI_COSTS = parse_ghost_qi_costs()

# 卡面文本里引用的费用变量名，必须与 WanJieRuLinCardModel.GhostQiCostVarName 一致。
GHOST_QI_COST_VAR = 'GhostQiCost'

# 费用行用「耗费」而不是「消耗」——因为「消耗」在本作里是 Exhaust 关键字的官方
# 译名。同一张牌上同时出现「消耗 2 点鬼气」和「消耗」会让玩家读成重复词、
# 分不清哪个是费用哪个是关键字（用户明确反馈过）。
_COST_VERB_ZH = '耗费'
_COST_VERB_EN = 'Spend'

# 官方文档（04-22-7 次要资源）推荐在卡面文本里用 secondaryResourceIcons() 渲染
# 资源图标：{Var:secondaryResourceIcons()} → 「鬼气图标 + 数字」。
_COST_FMT = 'secondaryResourceIcons()'


def ghost_qi_cost_line(cls, zh):
    """返回要插入卡面描述的鬼气费用行（无费用则返回 None）。

    固定费用卡 → 引用 DynamicVar，渲染成「鬼气图标 + 数字」，升级能跟着变。
    X 费用卡   → 写死「所有鬼气」（X 的实际值由框架的费用图标显示）。
    """
    cost = GHOST_QI_COSTS.get(cls)
    if cost is None:
        return None

    if zh:
        if cost == 'X':
            return '%s所有[gold]鬼气[/gold]。' % _COST_VERB_ZH
        return '%s {%s:%s} 点[gold]鬼气[/gold]。' % (_COST_VERB_ZH, GHOST_QI_COST_VAR, _COST_FMT)
    if cost == 'X':
        return '%s all [gold]Ghost Qi[/gold].' % _COST_VERB_EN
    return '%s {%s:%s} [gold]Ghost Qi[/gold].' % (_COST_VERB_EN, GHOST_QI_COST_VAR, _COST_FMT)

# ---------------------------------------------------------------- 卡牌
# key = 类名（UpperSnake 由转换函数生成）
# 值 = (中文名, 中文描述, 英文名, 英文描述)
#
# 规则（重要）：
# 1. 描述里 **不写** "升级后..." 之类的句子。升级后的数值由游戏读取
#    升级后的 CanonicalVars 自动渲染 —— 只要占位符写对，卡面会自动变化。
# 2. 每个 {Var:diff()} / {Var} 占位符必须能在该卡的 CanonicalVars 里找到同名变量，
#    否则卡面会显示成裸的 {Var} 或空白。
# 3. 能量费用不写在描述里（由费用图标负责显示）；但**鬼气费用必须写**，
#    因为 RitsuLib 的次要资源费用不会自动渲染到卡面上（见下方
#    parse_ghost_qi_costs / build_cards）。
CARDS = {
# ---- 基础 ----
'DaJi': ('打击', '造成 {Damage:diff()} 点伤害。',
         'Strike', 'Deal {Damage:diff()} damage.'),
'FangYu': ('防御', '获得 {Block:diff()} 点格挡。',
           'Defend', 'Gain {Block:diff()} Block.'),
'Hui': ('绘', '每耗费 [blue]1[/blue] 点鬼气，本回合获得 [blue]1[/blue] 点[gold]临时力量[/gold]和 [blue]1[/blue] 点[gold]临时敏捷[/gold]。',
        'Paint', 'For each [blue]1[/blue] Ghost Qi spent, gain [blue]1[/blue] [gold]Temporary Strength[/gold] and [blue]1[/blue] [gold]Temporary Dexterity[/gold] this turn.'),

# ---- 普通 ----
'FengBi': ('封笔', '获得 {Block:diff()} 点格挡。\n给予自身 {Weak:diff()} 层[gold]虚弱[/gold]。',
           'Seal the Brush', 'Gain {Block:diff()} Block.\nApply {Weak:diff()} [gold]Weak[/gold] to yourself.'),
'QinShi': ('侵蚀', '造成 {Damage:diff()} 点伤害。\n给予 {Vulnerable:diff()} 层[gold]易伤[/gold]。',
           'Corrode', 'Deal {Damage:diff()} damage.\nApply {Vulnerable:diff()} [gold]Vulnerable[/gold].'),
'MoDian': ('墨点', '获得 {Block:diff()} 点格挡。\n获得 {GhostQiGain:diff()} 点[gold]鬼气[/gold]。',
           'Ink Dot', 'Gain {Block:diff()} Block.\nGain {GhostQiGain:diff()} [gold]Ghost Qi[/gold].'),
'TuiGui': ('蜕鬼', '[gold]鬼气[/gold]为 [blue]0[/blue] 时才能打出。\n造成 {Damage:diff()} 点伤害，共 {Repeat:diff()} 次。',
           'Molt the Ghost', 'Can only be played while you have [blue]0[/blue] [gold]Ghost Qi[/gold].\nDeal {Damage:diff()} damage {Repeat:diff()} times.'),
'GuangMingQieGe': ('光明切割', '使目标失去 {StrengthLoss:diff()} 点[gold]力量[/gold]。\n[gold]消耗[/gold]。',
                   'Light Slash', 'Target loses {StrengthLoss:diff()} [gold]Strength[/gold].\n[gold]Exhaust[/gold].'),
'BiMoQingXie': ('笔墨倾泻', '对所有敌人造成 {Damage:diff()} 点伤害，共 X 次（X 为你消耗的[gold]能量[/gold]）。\n获得 X 点[gold]覆甲[/gold]（X 为能量与[gold]鬼气[/gold]之和）。',
                'Ink Torrent', 'Deal {Damage:diff()} damage to ALL enemies X times, where X is the [gold]Energy[/gold] spent.\nGain X [gold]Plating[/gold], where X is Energy plus [gold]Ghost Qi[/gold].'),
'ShenShiDuoShi': ('审时度势', '每耗费 [blue]1[/blue] 点鬼气，获得 [blue]1[/blue] 点[gold]能量[/gold]。\n抽 {Cards:diff()} 张牌。',
                  'Read the Room', 'For each [blue]1[/blue] Ghost Qi spent, gain [blue]1[/blue] [gold]Energy[/gold].\nDraw {Cards:diff()} card(s).'),
'ChiXuQinRao': ('持续侵扰', '造成 {Damage:diff()} 点伤害。\n给予 {Vulnerable:diff()} 层[gold]易伤[/gold]和 {Weak:diff()} 层[gold]虚弱[/gold]。',
                'Persistent Harassment', 'Deal {Damage:diff()} damage.\nApply {Vulnerable:diff()} [gold]Vulnerable[/gold] and {Weak:diff()} [gold]Weak[/gold].'),
'XiaoHuo': ('消火', '获得 {Block:diff()} 点格挡。\n给予自身 {Weak:diff()} 层[gold]虚弱[/gold]。',
            'Quench', 'Gain {Block:diff()} Block.\nApply {Weak:diff()} [gold]Weak[/gold] to yourself.'),
'LueDuo': ('掠夺', '造成 {Damage:diff()} 点伤害。\n获得 {GhostQiGain:diff()} 点[gold]鬼气[/gold]。',
           'Plunder', 'Deal {Damage:diff()} damage.\nGain {GhostQiGain:diff()} [gold]Ghost Qi[/gold].'),
'DaMengYiChang': ('大梦一场', '获得 {Block:diff()} 点格挡。\n下回合抽 {Cards:diff()} 张牌。',
                  'A Long Dream', 'Gain {Block:diff()} Block.\nNext turn, draw {Cards:diff()} card(s).'),
'DianXian': ('点，线', '每耗费 [blue]1[/blue] 点鬼气，此牌伤害 +[blue]3[/blue]。\n造成 {Damage:diff()} 点伤害，再加上该加值。',
             'Dot, Line', 'This card deals +[blue]3[/blue] damage for each [blue]1[/blue] Ghost Qi spent.\nDeal {Damage:diff()} damage, plus that bonus.'),

# ---- 罕见 ----
'YinYangGeHunXiao': ('阴阳割昏晓', '每打出 [blue]1[/blue] 张[gold]攻击牌[/gold]，抽 {DrawPerAttack:diff()} 张牌。\n每打出 [blue]1[/blue] 张[gold]技能牌[/gold]，随机[gold]消耗[/gold] {ChooseExhaust:diff()} 张手牌。',
                     'Yin and Yang Part the Twilight', 'Whenever you play an [gold]Attack[/gold], draw {DrawPerAttack:diff()} card(s).\nWhenever you play a [gold]Skill[/gold], randomly [gold]Exhaust[/gold] {ChooseExhaust:diff()} card(s) in your hand.'),
'GuiYingSenSen': ('鬼影森森', '每造成 {Threshold:diff()} 点伤害，获得 [blue]1[/blue] 点[gold]鬼气[/gold]。',
                  'Haunting Shadows', 'Every time you deal {Threshold:diff()} damage, gain [blue]1[/blue] [gold]Ghost Qi[/gold].'),
'TunShi': ('吞噬', '每获得 [blue]1[/blue] 点[gold]鬼气[/gold]，抽 [blue]1[/blue] 张牌。',
           'Devour', 'Whenever you gain [blue]1[/blue] [gold]Ghost Qi[/gold], draw [blue]1[/blue] card.'),
'HuaShen': ('化神', '每消耗 [blue]1[/blue] 点[gold]鬼气[/gold]，抽 [blue]1[/blue] 张牌并获得 [blue]1[/blue] 点[gold]能量[/gold]。',
            'Ascend to Godhood', 'Whenever you spend [blue]1[/blue] [gold]Ghost Qi[/gold], draw [blue]1[/blue] card and gain [blue]1[/blue] Energy.'),
'YinSen': ('阴森', '造成 {Damage:diff()} 点伤害，共 {Repeat:diff()} 次。\n[gold]消耗[/gold]。\n将 {Cards:diff()} 张[gold]阴森森[/gold]洗入抽牌堆。',
           'Sinister', 'Deal {Damage:diff()} damage {Repeat:diff()} times.\n[gold]Exhaust[/gold].\nShuffle {Cards:diff()} [gold]Sinister Shade[/gold] into your draw pile.'),
'HuiHuaFenGe': ('绘画分割', '造成 {Damage:diff()} 点伤害。\n若目标因此死亡，获得 {GhostQiGain:diff()} 点[gold]鬼气[/gold]。',
                'Painting Sliced Apart', 'Deal {Damage:diff()} damage.\nIf the target dies, gain {GhostQiGain:diff()} [gold]Ghost Qi[/gold].'),
'ShengWenZi': ('圣文字', '造成 {Damage:diff()} 点伤害。\n给予 {VulnerablePower:diff()} 层[gold]易伤[/gold]和 {WeakPower:diff()} 层[gold]虚弱[/gold]。',
               'Holy Script', 'Deal {Damage:diff()} damage.\nApply {VulnerablePower:diff()} [gold]Vulnerable[/gold] and {WeakPower:diff()} [gold]Weak[/gold].'),
'XiaBi': ('下笔', '造成 {Damage:diff()} 点伤害。\n下回合获得 {GhostQiGain:diff()} 点[gold]鬼气[/gold]。',
          'Set the Brush', 'Deal {Damage:diff()} damage.\nNext turn, gain {GhostQiGain:diff()} [gold]Ghost Qi[/gold].'),
'YouYouDianLong': ('悠悠电龙', '造成 {Damage:diff()} 点伤害。\n给予 {VulnerablePower:diff()} 层[gold]易伤[/gold]。\n重新打出你上回合打出的所有牌。',
                   'Languid Lightning Dragon', 'Deal {Damage:diff()} damage.\nApply {VulnerablePower:diff()} [gold]Vulnerable[/gold].\nReplay every card you played last turn.'),
'YanMoXiaBi': ('研磨下笔', '[gold]鬼气[/gold]大于 [blue]3[/blue] 时才能打出。\n造成 {Damage:diff()} 点伤害。',
               'Grind the Ink', 'Can only be played while you have more than [blue]3[/blue] [gold]Ghost Qi[/gold].\nDeal {Damage:diff()} damage.'),
'PiaoMiaoJianJue': ('飘渺剑诀', '造成 {Damage:diff()} 点伤害。\n随机获得一把剑：\n[gold]金剑[/gold]：回合开始时获得 [blue]3[/blue] 点[gold]活力[/gold]。\n[gold]木剑[/gold]：回合开始时回复 [blue]1[/blue] 点生命。\n[gold]水剑[/gold]：回合开始时，弃牌堆中一张攻击牌获得[gold]消耗[/gold]与单回合[gold]保留[/gold]。\n[gold]火剑[/gold]：回合开始时获得 [blue]1[/blue] 点[gold]临时力量[/gold]。\n[gold]土剑[/gold]：回合开始时获得 [blue]1[/blue] 点[gold]覆甲[/gold]。\n[gold]消耗[/gold]。',
                    'Ethereal Sword Art', 'Deal {Damage:diff()} damage.\nGain a random sword:\n[gold]Metal Sword[/gold]: gain [blue]3[/blue] [gold]Vigor[/gold] at the start of each turn.\n[gold]Wood Sword[/gold]: heal [blue]1[/blue] HP at the start of each turn.\n[gold]Water Sword[/gold]: at the start of each turn, an Attack in your discard pile gains [gold]Exhaust[/gold] and single-turn [gold]Retain[/gold].\n[gold]Fire Sword[/gold]: gain [blue]1[/blue] [gold]Temporary Strength[/gold] at the start of each turn.\n[gold]Earth Sword[/gold]: gain [blue]1[/blue] [gold]Plating[/gold] at the start of each turn.\n[gold]Exhaust[/gold].'),
'CengCengQinShi': ('层层侵蚀', '获得 {Block:diff()} 点格挡。\n若打出此牌后你的[gold]鬼气[/gold]为 [blue]0[/blue]，获得 {GhostQiGain:diff()} 点[gold]鬼气[/gold]。',
                   'Layered Erosion', 'Gain {Block:diff()} Block.\nIf you have [blue]0[/blue] [gold]Ghost Qi[/gold] after playing this, gain {GhostQiGain:diff()} [gold]Ghost Qi[/gold].'),
'WoRuoWeiGui': ('我若为鬼', '下回合获得的[gold]能量[/gold]全部转为[gold]鬼气[/gold]。\n下回合抽 {Cards:diff()} 张牌。\n[gold]消耗[/gold]。',
                'If I Were a Ghost', 'Next turn, all [gold]Energy[/gold] you would gain is converted into [gold]Ghost Qi[/gold].\nNext turn, draw {Cards:diff()} card(s).\n[gold]Exhaust[/gold].'),
'WoRuoWeiShen': ('我若为神', '下回合获得的[gold]鬼气[/gold]全部转为[gold]能量[/gold]。\n下回合可以免费打出 {FreeCards:diff()} 张牌。\n[gold]消耗[/gold]。',
                 'If I Were a God', 'Next turn, all [gold]Ghost Qi[/gold] you would gain is converted into [gold]Energy[/gold].\nNext turn, {FreeCards:diff()} card(s) can be played for free.\n[gold]Exhaust[/gold].'),
'ZanBiFengMang': ('暂避锋芒', '本回合你无法打出[gold]攻击牌[/gold]，且获得的[gold]格挡[/gold]翻倍。\n下回合获得 {Energy} 点[gold]能量[/gold]和 {GhostQiGain:diff()} 点[gold]鬼气[/gold]。',
                  'Bide Your Time', 'This turn you cannot play [gold]Attacks[/gold], and [gold]Block[/gold] you gain is doubled.\nNext turn, gain {Energy} Energy and {GhostQiGain:diff()} [gold]Ghost Qi[/gold].'),
'YaZhiYuWang': ('压制欲望', '下回合获得 {GhostQiGain:diff()} 点[gold]鬼气[/gold]。',
                'Suppress Desire', 'Next turn, gain {GhostQiGain:diff()} [gold]Ghost Qi[/gold].'),
'ShenShengBaoZou': ('神圣暴走', '本回合你造成的攻击伤害翻倍。\n每打出 {AttacksPerGhostQi:diff()} 张[gold]攻击牌[/gold]，消耗 [blue]1[/blue] 点[gold]鬼气[/gold]。',
                    'Divine Rampage', 'Attack damage you deal is doubled this turn.\nEvery {AttacksPerGhostQi:diff()} [gold]Attack[/gold] you play, spend [blue]1[/blue] [gold]Ghost Qi[/gold].'),
'DuZiShengJi': ('独自升级', '下回合随机获得 {Amount:diff()} 点[gold]能量[/gold]或 {Amount:diff()} 点[gold]鬼气[/gold]。\n[gold]消耗[/gold]。',
                'Solo Leveling', 'Next turn, randomly gain {Amount:diff()} [gold]Energy[/gold] or {Amount:diff()} [gold]Ghost Qi[/gold].\n[gold]Exhaust[/gold].'),
'DianLongHuFa': ('电龙护法', '本回合获得 {Block:diff()} 点格挡。\n下回合获得 {NextTurnBlock:diff()} 点格挡。',
                 'Lightning Dragon Ward', 'Gain {Block:diff()} Block this turn.\nGain {NextTurnBlock:diff()} Block next turn.'),
'HeiAnBiZhang': ('黑暗壁障', '本回合获得 {HeiAnBiZhangTempDexterityPower:diff()} 点[gold]临时敏捷[/gold]。\n获得 {Block:diff()} 点格挡。',
                 'Dark Barrier', 'Gain {HeiAnBiZhangTempDexterityPower:diff()} [gold]Temporary Dexterity[/gold] this turn.\nGain {Block:diff()} Block.'),

# ---- 稀有 ----
'DianLongXingTai': ('电龙形态', '回合开始时，获得 [blue]10[/blue] 点[gold]活力[/gold]和 [blue]3[/blue] 点[gold]临时力量[/gold]。\n[gold]虚无[/gold]。',
                    'Lightning Dragon Form', 'At the start of each turn, gain [blue]10[/blue] [gold]Vigor[/gold] and [blue]3[/blue] [gold]Temporary Strength[/gold].\n[gold]Ethereal[/gold].'),
'LiGuiFuSu': ('厉鬼复苏', '每回合获得的[gold]能量[/gold]全部转为等量[gold]鬼气[/gold]。\n消耗掉所有[gold]能量牌[/gold]。\n每回合开始时发现一张耗费[gold]鬼气[/gold]的牌，它可以免费打出一次。',
              'Vengeful Ghost Revival', 'All [gold]Energy[/gold] you gain each turn is converted into an equal amount of [gold]Ghost Qi[/gold].\n[gold]Exhaust[/gold] all [gold]Energy cards[/gold].\nAt the start of each turn, discover a card that costs [gold]Ghost Qi[/gold]; it can be played for free once.'),
'GuangMingYuYan': ('光明预言', '每消耗 [blue]1[/blue] 点[gold]鬼气[/gold]，失去 {HpPerGhostQi:diff()} 点生命并对随机敌人造成等量伤害。',
                   'Prophecy of Light', 'Whenever you spend [blue]1[/blue] [gold]Ghost Qi[/gold], lose {HpPerGhostQi:diff()} HP and deal that much damage to a random enemy.'),
'GuiYu': ('鬼域', '每回合第 [blue]1[/blue] 张[gold]攻击牌[/gold]伤害翻倍。\n每回合第 [blue]3[/blue] 张牌本回合费用变为 [blue]0[/blue]。',
          'Ghost Domain', 'The first [gold]Attack[/gold] each turn deals double damage.\nThe [blue]3[/blue]rd card each turn costs [blue]0[/blue] this turn.'),
'YangGuangPuZhao': ('阳光普照', '[gold]鬼气[/gold]为 [blue]0[/blue] 时才能打出。\n造成 {Damage:diff()} 点伤害。\n击晕所有敌人。\n[gold]消耗[/gold]。',
                   'The Sun Shines Everywhere', 'Can only be played while you have [blue]0[/blue] [gold]Ghost Qi[/gold].\nDeal {Damage:diff()} damage.\nStun ALL enemies.\n[gold]Exhaust[/gold].'),
'YouRanGuiHuo': ('幽然鬼火', '造成 {Damage:diff()} 点伤害。\n给予 {VulnerablePower:diff()} 层[gold]易伤[/gold]。',
                 'Eerie Will-o\'-Wisp', 'Deal {Damage:diff()} damage.\nApply {VulnerablePower:diff()} [gold]Vulnerable[/gold].'),
'XianMian': ('线，面', '对所有敌人造成 X+{Bonus:diff()} 点伤害，共 {Repeat:diff()} 次（X 为你耗费的鬼气）。',
             'Line, Plane', 'Deal X+{Bonus:diff()} damage to ALL enemies {Repeat:diff()} times, where X is the Ghost Qi spent.'),
'MangXinJian': ('盲心剑', '造成 {Damage:diff()} 点伤害。\n使目标失去 [blue]8[/blue] 点[gold]力量[/gold]。\n[gold]消耗[/gold]。',
                'Blind Heart Sword', 'Deal {Damage:diff()} damage.\nTarget loses [blue]8[/blue] [gold]Strength[/gold].\n[gold]Exhaust[/gold].'),
'AoMan': ('傲慢', '造成 {Damage:diff()} 点伤害。\n本回合内，你每次造成伤害都获得等量[gold]鬼气[/gold]。',
          'Arrogance', 'Deal {Damage:diff()} damage.\nThis turn, damage you deal grants an equal amount of [gold]Ghost Qi[/gold].'),
'ShiTong': ('尸瞳', '本回合每打出 [blue]1[/blue] 张牌，获得 {GhostQiPerCard:diff()} 点[gold]鬼气[/gold]。',
            'Corpse Eye', 'This turn, for every card you play, gain {GhostQiPerCard:diff()} [gold]Ghost Qi[/gold].'),
'PingXingShiJie': ('平行世界', '选择 X 张牌置入弃牌堆。\n抽 X+{BonusDraw:diff()} 张牌。\n获得 {Energy} 点[gold]能量[/gold]。',
                   'Parallel World', 'Choose X cards and put them into your discard pile.\nDraw X+{BonusDraw:diff()} cards.\nGain {Energy} Energy.'),
'AoJiao': ('傲娇', '抽 {Cards:diff()} 张牌。\n获得等于这些牌伤害总和的格挡。\n[gold]消耗[/gold]。',
           'Tsundere', 'Draw {Cards:diff()} cards.\nGain Block equal to the total damage of those cards.\n[gold]Exhaust[/gold].'),
'Jian': ('剑！', '接下来 {Turns:diff()} 个回合，每回合开始获得 {SwordsPerTurn:diff()} 把剑的效果。\n[gold]消耗[/gold]。',
        'Sword!', 'For the next {Turns:diff()} turns, gain the effect of {SwordsPerTurn:diff()} random swords at the start of each turn.\n[gold]Exhaust[/gold].'),
'WoBuWanLe': ('我不玩了', '之后回合无法获得[gold]鬼气[/gold]。\n接下来 X+{BonusFree:diff()} 张牌可以免费打出。',
              'I Quit', 'You can no longer gain [gold]Ghost Qi[/gold] on future turns.\nThe next X+{BonusFree:diff()} cards can be played for free.'),

# ---- 先古 / 事件 ----
'MoRanJiangShan': ('墨染江山', '本回合获得 X+{Bonus:diff()} 点[gold]临时力量[/gold]和 X+{Bonus:diff()} 点[gold]临时敏捷[/gold]。\n[gold]保留[/gold]。',
                   'Ink-Stained Realm', 'Gain X+{Bonus:diff()} [gold]Temporary Strength[/gold] and X+{Bonus:diff()} [gold]Temporary Dexterity[/gold] this turn.\n[gold]Retain[/gold].'),
'GuiQiSenSen': ('鬼气森森', '你接下来的回合不再获得[gold]能量[/gold]，改为获得等量[gold]鬼气[/gold]。\n所有[gold]能量牌[/gold]耗能变为 [blue]0[/blue] 并获得[gold]消耗[/gold]。',
               'Ghost Qi Abounds', 'On future turns you no longer gain [gold]Energy[/gold]; instead gain an equal amount of [gold]Ghost Qi[/gold].\nAll [gold]Energy cards[/gold] cost [blue]0[/blue] and gain [gold]Exhaust[/gold].'),

# ---- 衍生物 ----
'YinSenSen': ('阴森森', '造成 {Damage:diff()} 点伤害，共 {Repeat:diff()} 次。',
              'Shade of Shade', 'Deal {Damage:diff()} damage {Repeat:diff()} times.'),
}

# ---------------------------------------------------------------- 能力
POWERS = {
'EnergyToGhostQiPower': ('能量转鬼气',
    '你接下来获得的[gold]能量[/gold]将转为等量[gold]鬼气[/gold]。',
    'Energy to Ghost Qi',
    'All [gold]Energy[/gold] you would gain is converted into an equal amount of [gold]Ghost Qi[/gold].'),
'GhostQiToEnergyPower': ('鬼气转能量',
    '你接下来获得的[gold]鬼气[/gold]将转为等量[gold]能量[/gold]。',
    'Ghost Qi to Energy',
    'All [gold]Ghost Qi[/gold] you would gain is converted into an equal amount of [gold]Energy[/gold].'),
'GhostQiNextTurnPower': ('蓄势鬼气',
    '下回合开始时，获得 {Amount} 点[gold]鬼气[/gold]。',
    'Stored Ghost Qi',
    'At the start of next turn, gain {Amount} [gold]Ghost Qi[/gold].'),
'GuiYingSenSenPower': ('鬼影森森',
    '每造成 {Threshold} 点伤害，获得 [blue]1[/blue] 点[gold]鬼气[/gold]。',
    'Haunting Shadows',
    'Every time you deal {Threshold} damage, gain [blue]1[/blue] [gold]Ghost Qi[/gold].'),
'TunShiPower': ('吞噬',
    '每获得 [blue]1[/blue] 点[gold]鬼气[/gold]，抽 [blue]1[/blue] 张牌。',
    'Devour',
    'Whenever you gain [blue]1[/blue] [gold]Ghost Qi[/gold], draw [blue]1[/blue] card.'),
'HuaShenPower': ('化神',
    '每消耗 [blue]1[/blue] 点[gold]鬼气[/gold]，抽 [blue]1[/blue] 张牌并获得 [blue]1[/blue] 点[gold]能量[/gold]。',
    'Ascend to Godhood',
    'Whenever you spend [blue]1[/blue] [gold]Ghost Qi[/gold], draw [blue]1[/blue] card and gain [blue]1[/blue] Energy.'),
'YinYangGeHunXiaoPower': ('阴阳割昏晓',
    '每打出 [blue]1[/blue] 张[gold]攻击牌[/gold]，抽 {Amount} 张牌。\n每打出 [blue]1[/blue] 张[gold]技能牌[/gold]，[gold]消耗[/gold] [blue]1[/blue] 张手牌。',
    'Yin and Yang Part the Twilight',
    'Whenever you play an [gold]Attack[/gold], draw {Amount} card(s).\nWhenever you play a [gold]Skill[/gold], [gold]Exhaust[/gold] [blue]1[/blue] card in your hand.'),
'BideDefensivelyPower': ('暂避锋芒',
    '本回合你无法打出[gold]攻击牌[/gold]，获得的[gold]格挡[/gold]翻倍。',
    'Bide Your Time',
    'This turn you cannot play [gold]Attacks[/gold], and [gold]Block[/gold] you gain is doubled.'),
'DivineRampagePower': ('神圣暴走',
    '本回合你造成的攻击伤害翻倍。每打出 {AttacksPerGhostQi} 张[gold]攻击牌[/gold]消耗 [blue]1[/blue] 点[gold]鬼气[/gold]。',
    'Divine Rampage',
    'Attack damage you deal is doubled this turn. Every {AttacksPerGhostQi} [gold]Attack[/gold] you play, spend [blue]1[/blue] [gold]Ghost Qi[/gold].'),
'SoloLevelUpPower': ('独自升级',
    '下回合开始时，随机获得 {Amount} 点[gold]能量[/gold]或 {Amount} 点[gold]鬼气[/gold]。',
    'Solo Leveling',
    'At the start of next turn, randomly gain {Amount} [gold]Energy[/gold] or {Amount} [gold]Ghost Qi[/gold].'),
'DianLongFormPower': ('电龙形态',
    '回合开始时获得 [blue]10[/blue] 点[gold]活力[/gold]和 [blue]3[/blue] 点[gold]临时力量[/gold]。',
    'Lightning Dragon Form', 'At the start of each turn, gain [blue]10[/blue] [gold]Vigor[/gold] and [blue]3[/blue] [gold]Temporary Strength[/gold].'),
'GuiYuPower': ('鬼域',
    '每回合第 [blue]1[/blue] 张[gold]攻击牌[/gold]伤害翻倍，第 [blue]1[/blue] 张[gold]技能牌[/gold]免费并抽 [blue]2[/blue] 张牌，第 [blue]3[/blue] 张牌费用为 [blue]0[/blue]，第 [blue]4[/blue] 张牌获得[gold]重放[/gold]。',
    'Ghost Domain', 'The first [gold]Attack[/gold] each turn deals double damage; the first [gold]Skill[/gold] is free and draws [blue]2[/blue] cards; the [blue]3[/blue]rd card costs [blue]0[/blue]; the [blue]4[/blue]th gains [gold]Replay[/gold].'),
'ShiTongPower': ('尸瞳',
    '本回合每打出一张牌，获得 [blue]1[/blue] 点[gold]鬼气[/gold]。',
    'Corpse Eye',
    'This turn, for every card you play, gain [blue]1[/blue] [gold]Ghost Qi[/gold].'),
'AoManPower': ('傲慢',
    '本回合内，你每次造成伤害都获得等量[gold]鬼气[/gold]。',
    'Arrogance',
    'This turn, damage you deal grants an equal amount of [gold]Ghost Qi[/gold].'),
'SwordStormPower': ('剑雨',
    '回合开始时随机获得 {Amount} 把剑的效果。',
    'Sword Storm',
    'At the start of each turn, gain the effect of {Amount} random swords.'),
'WoBuWanLePower': ('我不玩了',
    '你无法获得[gold]鬼气[/gold]。接下来 {Amount} 张牌可以免费打出。',
    'I Quit',
    'You cannot gain [gold]Ghost Qi[/gold]. The next {Amount} cards can be played for free.'),
'GuangMingYuYanPower': ('光明预言',
    '每消耗 [blue]1[/blue] 点[gold]鬼气[/gold]，失去 {Amount} 点生命并对随机敌人造成等量伤害。',
    'Prophecy of Light',
    'Whenever you spend [blue]1[/blue] [gold]Ghost Qi[/gold], lose {Amount} HP and deal that much damage to a random enemy.'),
'LastTurnCardsPower': ('回响',
    '记录上回合打出的牌。',
    'Echo',
    'Records the cards you played last turn.'),
'JinJianPower': ('金剑',
    '回合开始时获得 [blue]3[/blue] 点[gold]活力[/gold]。',
    'Metal Sword', 'At the start of each turn, gain [blue]3[/blue] [gold]Vigor[/gold].'),
'MuJianPower': ('木剑',
    '回合开始时回复 [blue]1[/blue] 点生命。',
    'Wood Sword', 'At the start of each turn, heal [blue]1[/blue] HP.'),
'ShuiJianPower': ('水剑',
    '回合开始时，弃牌堆中随机一张[gold]攻击牌[/gold]获得[gold]消耗[/gold]与单回合[gold]保留[/gold]。',
    'Water Sword', 'At the start of each turn, a random [gold]Attack[/gold] in your discard pile gains [gold]Exhaust[/gold] and single-turn [gold]Retain[/gold].'),
'HuoJianPower': ('火剑',
    '回合开始时获得 [blue]1[/blue] 点[gold]临时力量[/gold]。',
    'Fire Sword', 'At the start of each turn, gain [blue]1[/blue] [gold]Temporary Strength[/gold].'),
'TuJianPower': ('土剑',
    '回合开始时获得 [blue]1[/blue] 点[gold]覆甲[/gold]。',
    'Earth Sword', 'At the start of each turn, gain [blue]1[/blue] [gold]Plating[/gold].'),
}

# 临时能力包装（多个来源共用一条文本 + 一张图）。
# 键名由代码里的 WanJieTempAppliedPower.LocStem 指定，
# 所以这里用显式键而不是由类名推导。
# 对应的图片沿用 images/powers/WanJieTempStrengthPower.png / WanJieTempDexterityPower.png。
TEMP_APPLIED_POWERS = {
    'WAN_JIE_RU_LIN_POWER_TEMP_STRENGTH': ('临时力量',
        '本回合内提供 {Amount} 点[gold]力量[/gold]，回合结束时撤回。',
        'Temporary Strength',
        'Grants {Amount} [gold]Strength[/gold] this turn; removed at end of turn.'),
    'WAN_JIE_RU_LIN_POWER_TEMP_DEXTERITY': ('临时敏捷',
        '本回合内提供 {Amount} 点[gold]敏捷[/gold]，回合结束时撤回。',
        'Temporary Dexterity',
        'Grants {Amount} [gold]Dexterity[/gold] this turn; removed at end of turn.'),
}

# ---------------------------------------------------------------- 遗物
RELICS = {
'GuiMo': ('鬼墨',
    '[gold]鬼气[/gold]：每场战斗开始时获得 [blue]1[/blue] 点鬼气，每回合开始时获得 [blue]1[/blue] 点鬼气。',
    '亡者落笔，墨即是魂。',
    'Ghost Ink',
    '[gold]Ghost Qi[/gold]: at the start of each combat gain [blue]1[/blue] Ghost Qi; at the start of each turn gain [blue]1[/blue] Ghost Qi.',
    'The dead dip the brush; the ink is the soul.'),
'JinSiGuiMo': ('金丝鬼墨',
    '[gold]鬼气[/gold]：每场战斗开始时获得 [blue]3[/blue] 点鬼气，每回合开始时获得 [blue]1[/blue] 点鬼气。',
    '以金丝缚住游魂，墨迹再无干涸之日。',
    'Golden Silk Ghost Ink',
    '[gold]Ghost Qi[/gold]: at the start of each combat gain [blue]3[/blue] Ghost Qi; at the start of each turn gain [blue]1[/blue] Ghost Qi.',
    'Bind a wandering soul with golden thread, and the ink shall never dry.'),
'YiBaXiaoJian': ('一把小剑',
    '战斗开始时随机获得一把剑的效果。',
    '小，但很锋利。',
    'A Small Sword',
    'At the start of combat, gain the effect of a random sword.',
    'Small, but sharp.'),
'TianShiSan': ('天师伞',
    '每消耗 [blue]4[/blue] 点[gold]鬼气[/gold]，获得 [blue]1[/blue] 点[gold]能量[/gold]。\n每消耗 [blue]4[/blue] 点[gold]能量[/gold]，获得 [blue]1[/blue] 点[gold]鬼气[/gold]。',
    '道人遗物，撑开时阴阳两界皆不得近身。',
    'Celestial Master\'s Umbrella',
    'Every [blue]4[/blue] [gold]Ghost Qi[/gold] spent grants [blue]1[/blue] [gold]Energy[/gold].\nEvery [blue]4[/blue] [gold]Energy[/gold] spent grants [blue]1[/blue] [gold]Ghost Qi[/gold].',
    'A relic of the Daoist master; opened, it keeps both worlds at bay.'),
'BeiLeiMao': ('贝雷帽',
    '战斗开始时获得一张随机[gold]技能牌[/gold]，该牌可以免费打出一次，随后[gold]消耗[/gold]。',
    '戴上它，灵感来得很突然。',
    'Beret',
    'At the start of combat, gain a random [gold]Skill[/gold]. It can be played for free once, then [gold]Exhausts[/gold].',
    'Put it on, and inspiration strikes out of nowhere.'),
}

# ---------------------------------------------------------------- 角色
CHAR = 'WAN_JIE_RU_LIN_CHARACTER_WAN_JIE_RU_LIN_CHARACTER'
CHARACTERS_ZH = {
    CHAR + '.title': '万界如林',
    CHAR + '.titleObject': '万界如林',
    CHAR + '.pronounSubject': '她',
    CHAR + '.pronounObject': '她',
    CHAR + '.pronounPossessive': '她的',
    CHAR + '.possessiveAdjective': '她的',
    CHAR + '.description': '“林”的身份一直是一个谜。\n有关于她的故事有很多，或许会有真的。\n\n[gold]鬼气[/gold]：她的第二资源，如墨般在牌组间流转。',
    CHAR + '.selectMessage': '墨色漫开，故事开始了。',
    CHAR + '.flavor': '万界如林，落笔成真。',
    CHAR + '.defeatMessage': '故事……还没讲完。',
    CHAR + '.victoryMessage': '这一卷，写完了。',
    CHAR + '.eventDeathPrevention': '我还不能在这里结束。',
    CHAR + '.goldMonologue': '这些金币，也算故事的一部分吧。',
    CHAR + '.aromaPrinciple': '[sine][blue]……有墨的味道。很旧的墨，还有很多故事没讲完。[/blue][/sine]',
    CHAR + '.banter.alive.endTurnPing': '再想想。',
    CHAR + '.banter.dead.endTurnPing': '先搁笔了。',
    CHAR + '.cardsModifierTitle': '万界如林卡牌',
    CHAR + '.cardsModifierDescription': '万界如林的卡牌现在会出现在奖励和商店中。',
    CHAR + '.unlockText': '用[pink]{Prerequisite}[/pink]进行一局游戏来解锁这个角色。',
}
CHARACTERS_EN = {
    CHAR + '.title': 'Wan Jie Ru Lin',
    CHAR + '.titleObject': 'Wan Jie Ru Lin',
    CHAR + '.pronounSubject': 'she',
    CHAR + '.pronounObject': 'her',
    CHAR + '.pronounPossessive': 'her',
    CHAR + '.possessiveAdjective': 'her',
    CHAR + '.description': 'The identity of "Lin" has always been a mystery.\nThere are many stories about her - perhaps some of them are true.\n\n[gold]Ghost Qi[/gold]: her second resource, flowing through the deck like ink.',
    CHAR + '.selectMessage': 'The ink spreads, and the story begins.',
    CHAR + '.flavor': 'A thousand realms become a forest; the brush falls, and it is real.',
    CHAR + '.defeatMessage': 'The story... is not finished yet.',
    CHAR + '.victoryMessage': 'This scroll is written to the end.',
    CHAR + '.eventDeathPrevention': 'I cannot end here.',
    CHAR + '.goldMonologue': 'Even these coins are part of the story.',
    CHAR + '.aromaPrinciple': "[sine][blue]...Smells like ink. Old ink, with too many stories left untold.[/blue][/sine]",
    CHAR + '.banter.alive.endTurnPing': 'Let me think.',
    CHAR + '.banter.dead.endTurnPing': 'Setting down the brush.',
    CHAR + '.cardsModifierTitle': 'Wan Jie Ru Lin Cards',
    CHAR + '.cardsModifierDescription': 'Wan Jie Ru Lin cards can now appear in rewards and shops.',
    CHAR + '.unlockText': 'Complete a run as [pink]{Prerequisite}[/pink] to unlock this character.',
}

# ---------------------------------------------------------------- 先古对话
# 只放先古事件自己的对话键。
# 先古**选项**文案不在这里：选项用 cards 表的
# WAN_JIE_RU_LIN_CARD_MO_RAN_JIANG_SHAN_OPTION.{title,description}
# （见 build_cards），因为事件界面渲染选项文案时拿不到卡牌 DynamicVars。
TALK_ZH = {
    'NEOW.talk.' + CHAR + '.0-0.char': '我准备好了。',
    'NEOW.talk.' + CHAR + '.0-0.next': '继续',
    'NEOW.talk.' + CHAR + '.0-1.ancient': '[sine]去吧……带着你的墨……在这万界里画出一条路来……[/sine]',
    'DARV.talk.' + CHAR + '.0-0.char': '这里的空白，正好落笔。',
    'DARV.talk.' + CHAR + '.0-0.next': '继续',
    'DARV.talk.' + CHAR + '.0-1.ancient': '空白才好啊！想画什么，就画什么！',
}
TALK_EN = {
    'NEOW.talk.' + CHAR + '.0-0.char': 'I am ready.',
    'NEOW.talk.' + CHAR + '.0-0.next': 'Continue',
    'NEOW.talk.' + CHAR + '.0-1.ancient': '[sine]Go... take your ink... and draw a path through the thousand realms...[/sine]',
    'DARV.talk.' + CHAR + '.0-0.char': 'This blank space is just right for the brush.',
    'DARV.talk.' + CHAR + '.0-0.next': 'Continue',
    'DARV.talk.' + CHAR + '.0-1.ancient': 'Blank is good! Paint whatever you want!',
}

# ---------------------------------------------------------------- 通用 / 关键词
COMMON_ZH = {
    'WAN_JIE_RU_LIN_GHOST_QI': '鬼气',
    'WAN_JIE_RU_LIN_GHOST_QI_TITLE': '鬼气',
    'WAN_JIE_RU_LIN_GHOST_QI_DESC': '万界如林的第二资源。如能量般用于支付部分卡牌的费用。\n一场战斗内跨回合保留，进入下一场战斗时重置为 [blue]1[/blue] 点，回合开始时不会自动恢复。',
    'WAN_JIE_RU_LIN_CARD_YIN_YANG_GE_HUN_XIAO.exhaustPrompt': '选择一张牌消耗',
    'WAN_JIE_RU_LIN_CARD_YIN_YANG_GE_HUN_XIAO.selectPrompt': '选择一张牌',
}
COMMON_EN = {
    'WAN_JIE_RU_LIN_GHOST_QI': 'Ghost Qi',
    'WAN_JIE_RU_LIN_GHOST_QI_TITLE': 'Ghost Qi',
    'WAN_JIE_RU_LIN_GHOST_QI_DESC': "Wan Jie Ru Lin's secondary resource. Spent like Energy to pay for certain cards. It is kept between turns within a combat, but resets to [blue]1[/blue] when a new combat begins, and does not refill automatically at the start of each turn.",
    'WAN_JIE_RU_LIN_CARD_YIN_YANG_GE_HUN_XIAO.exhaustPrompt': 'Choose a card to Exhaust',
    'WAN_JIE_RU_LIN_CARD_YIN_YANG_GE_HUN_XIAO.selectPrompt': 'Choose a card',
}

# ---------------------------------------------------------------- 悬停提示表
# 重要：鬼气的悬停提示会被框架放在 base 表 `static_hover_tips` 里查找，
# 只会去 mod 的 `localization/{lang}/static_hover_tips.json` 合并。
# 放进 cards.json 是找不到的（日志会刷 Missing localization key ...
# in table 'static_hover_tips'）。所以这里单独出一个同名的表文件。
#
# 同时提供两套键名，因为框架在不同路径下分别用：
#   - titleKey / descriptionKey（我们在 ModResources 里显式指定的）
#   - GetLocString  → WAN_JIE_RU_LIN_GHOST_QI_TITLE / _DESC
#   - GetRawText    → WAN_JIE_RU_LIN_GHOST_QI.title / .description
#     （来自 GhostQiGainVarOf 的 WithSharedTooltip("WAN_JIE_RU_LIN_GHOST_QI")）
#   - 官方文档说 titleKey/descriptionKey 省略时会按 {resourceId}.title/.description
#     自动推导；resourceId 实测为 WAN_JIE_RU_LIN_SECONDARY_RESOURCE_GHOST_QI。
#     两条路径都补上，避免任何一条回退到裸键名。
_GHOST_QI_RES_ID = 'WAN_JIE_RU_LIN_SECONDARY_RESOURCE_GHOST_QI'

_HOVER_QI_ZH_TITLE = '鬼气'
_HOVER_QI_ZH_DESC = (
    '万界如林的第二资源，类似[gold]星辉[/gold]：独立于能量，用于支付部分卡牌的费用。\n'
    '一场战斗内跨回合保留，进入下一场战斗时重置为 [blue]1[/blue] 点；回合开始时不会自动恢复。'
)
_HOVER_QI_EN_TITLE = 'Ghost Qi'
_HOVER_QI_EN_DESC = (
    "Wan Jie Ru Lin's secondary resource, similar to [gold]Stars[/gold]: it is independent of Energy "
    "and is spent to pay the cost of certain cards.\n"
    "It is kept between turns within a combat, but resets to [blue]1[/blue] when a new combat begins, "
    "and does not refill automatically at the start of each turn."
)

STATIC_HOVER_TIPS_ZH = {
    'WAN_JIE_RU_LIN_GHOST_QI_TITLE': _HOVER_QI_ZH_TITLE,
    'WAN_JIE_RU_LIN_GHOST_QI_DESC': _HOVER_QI_ZH_DESC,
    'WAN_JIE_RU_LIN_GHOST_QI.title': _HOVER_QI_ZH_TITLE,
    'WAN_JIE_RU_LIN_GHOST_QI.description': _HOVER_QI_ZH_DESC,
    _GHOST_QI_RES_ID + '.title': _HOVER_QI_ZH_TITLE,
    _GHOST_QI_RES_ID + '.description': _HOVER_QI_ZH_DESC,
}
STATIC_HOVER_TIPS_EN = {
    'WAN_JIE_RU_LIN_GHOST_QI_TITLE': _HOVER_QI_EN_TITLE,
    'WAN_JIE_RU_LIN_GHOST_QI_DESC': _HOVER_QI_EN_DESC,
    'WAN_JIE_RU_LIN_GHOST_QI.title': _HOVER_QI_EN_TITLE,
    'WAN_JIE_RU_LIN_GHOST_QI.description': _HOVER_QI_EN_DESC,
    _GHOST_QI_RES_ID + '.title': _HOVER_QI_EN_TITLE,
    _GHOST_QI_RES_ID + '.description': _HOVER_QI_EN_DESC,
}


def upper_snake(name):
    """类名 → 本地化键用的 UPPER_SNAKE。

    DaJi                -> DA_JI
    YinYangGeHunXiao    -> YIN_YANG_GE_HUN_XIAO
    AoJiao              -> AO_JIAO

    注意 trailing 的 "Power" 不拆：
        YinYangGeHunXiaoPower -> YIN_YANG_GE_HUN_XIAO_POWER
        （不是 ..._XIAO_POWER_POWER）
    因为键模板本身已经带了 _POWER_ 前缀，游戏里的类名也是
    "YinYangGeHunXiaoPower"，两者拼起来必须只出现一次 POWER。
    """
    # 去掉结尾的 Power 再转换，最后手工接回。
    stem, tail = name, ''
    if name.endswith('Power') and len(name) > len('Power'):
        stem, tail = name[:-len('Power')], '_POWER'

    out = []
    for i, ch in enumerate(stem):
        if ch.isupper() and i > 0:
            prev = stem[i - 1]
            nxt = stem[i + 1] if i + 1 < len(stem) else ''
            if prev.islower() or (prev.isupper() and nxt.islower()):
                out.append('_')
        out.append(ch.upper())
    return ''.join(out) + tail


def build_cards(zh):
    d = {}
    for cls, v in CARDS.items():
        zh_name, zh_desc, en_name, en_desc = v
        key = 'WAN_JIE_RU_LIN_CARD_' + upper_snake(cls)
        name = zh_name if zh else en_name
        desc = zh_desc if zh else en_desc

        # 把「耗费 N 点鬼气」插到描述最前面（bug 9）。
        # 费用行是唯一来源，CARDS 里不再手写；这样牌面永远等于代码实收。
        line = ghost_qi_cost_line(cls, zh)
        if line and line not in desc:
            desc = line + '\n' + desc

        d[key + '.title'] = name
        d[key + '.description'] = desc
        d[key + '.smartDescription'] = desc

    # 先古事件里注入的「墨染江山」选项文案。
    # ★ 这里**绝对不能**出现 {占位符}：事件界面（NEventOptionButton）渲染选项文案时
    #   只注入事件变量，拿不到卡牌的 DynamicVars，任何 {Var} 都会解析失败并刷日志
    #   "No source extension could handle the selector named ..."。
    d['WAN_JIE_RU_LIN_CARD_MO_RAN_JIANG_SHAN_OPTION.title'] = '墨染江山' if zh else 'Ink-Stained Realm'
    d['WAN_JIE_RU_LIN_CARD_MO_RAN_JIANG_SHAN_OPTION.description'] = (
        '将「墨染江山」加入你的牌组。\n'
        '耗费所有[gold]鬼气[/gold]；本回合获得 X+3 点[gold]临时力量[/gold]和 X+3 点[gold]临时敏捷[/gold]。\n'
        '[gold]保留[/gold]。'
        if zh else
        'Add "Ink-Stained Realm" to your deck.\n'
        'Spend all [gold]Ghost Qi[/gold]; gain X+3 [gold]Temporary Strength[/gold] and X+3 [gold]Temporary Dexterity[/gold] this turn.\n'
        '[gold]Retain[/gold].')
    return d


def write(path, obj):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with io.open(path, 'w', encoding='utf-8', newline='\n') as f:
        json.dump(obj, f, ensure_ascii=False, indent=2)
        f.write('\n')
    print('  wrote %-46s %4d keys' % (os.path.relpath(path, ROOT), len(obj)))


for lang in ('zhs', 'eng'):
    zh = (lang == 'zhs')
    base = os.path.join(ROOT, lang)
    print('[' + lang + ']')

    cards = build_cards(zh)
    cards.update(COMMON_ZH if zh else COMMON_EN)
    write(os.path.join(base, 'cards.json'), dict(sorted(cards.items())))

    relics = {}
    for cls, v in RELICS.items():
        z_name, z_desc, z_flav, e_name, e_desc, e_flav = v
        k = 'WAN_JIE_RU_LIN_RELIC_' + upper_snake(cls)
        relics[k + '.title'] = z_name if zh else e_name
        relics[k + '.description'] = z_desc if zh else e_desc
        relics[k + '.flavor'] = z_flav if zh else e_flav
    write(os.path.join(base, 'relics.json'), dict(sorted(relics.items())))

    powers = {}
    for cls, v in POWERS.items():
        z_name, z_desc, e_name, e_desc = v
        k = 'WAN_JIE_RU_LIN_POWER_' + upper_snake(cls)
        powers[k + '.title'] = z_name if zh else e_name
        powers[k + '.description'] = z_desc if zh else e_desc
        powers[k + '.smartDescription'] = z_desc if zh else e_desc
    # 临时能力包装：键名是代码里写死的（WanJieTempAppliedPower.LocStem），
    # 不是由类名推导的，所以单独补齐。
    for k, v in TEMP_APPLIED_POWERS.items():
        z_name, z_desc, e_name, e_desc = v
        powers[k + '.title'] = z_name if zh else e_name
        powers[k + '.description'] = z_desc if zh else e_desc
        powers[k + '.smartDescription'] = z_desc if zh else e_desc
    write(os.path.join(base, 'powers.json'), dict(sorted(powers.items())))

    write(os.path.join(base, 'characters.json'),
          dict(sorted((CHARACTERS_ZH if zh else CHARACTERS_EN).items())))
    write(os.path.join(base, 'ancients.json'),
          dict(sorted((TALK_ZH if zh else TALK_EN).items())))

    # 鬼气的悬停提示必须落在 base 表 static_hover_tips 里（按文件名合并）。
    write(os.path.join(base, 'static_hover_tips.json'),
          dict(sorted((STATIC_HOVER_TIPS_ZH if zh else STATIC_HOVER_TIPS_EN).items())))

print('done')
