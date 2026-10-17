# -*- coding: utf-8 -*-
"""gen_loc_v09.py — 万界如林 v0.9 全新 90 张卡本地化生成器。

产物: localization/{zhs,eng}/{cards,powers,relics,characters,static_hover_tips,card_keywords}.json
真相源: 本文件的 CARDS / POWERS / RELIC_ZH / RELIC_EN / CHARACTER_* / TIPS_* / KW_*
"""
import io, os, re, json

ROOT = r'C:/Users/wangx/Documents/Default Project/WanJieRuLin/WanJieRuLin/localization'

PFX = 'WAN_JIE_RU_LIN_'


def snake(cls):
    return re.sub(r'(?<!^)(?=[A-Z])', '_', cls).upper()


def write(path, data):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    io.open(path, 'w', encoding='utf-8', newline='\n').write(
        json.dumps(dict(sorted(data.items())), ensure_ascii=False, indent=2))
    print('  wrote %-44s %d keys' % (os.path.relpath(path, ROOT), len(data)))


# ============================================================================
# 卡牌：cls -> (中文名, 中文描述, 英文名, 英文描述)
# 描述里用 {Var:diff()} 占位符展示可升级数值。
# ============================================================================
CARDS = {
    # ---- Basic ----
    'DaJi': ('打击', '造成 {Damage:diff()} 点伤害。', 'Strike', 'Deal {Damage:diff()} damage.'),
    'FangYu': ('防御', '获得 {Block:diff()} 点格挡。', 'Defend', 'Gain {Block:diff()} Block.'),
    'ZuoSui': ('作祟', '[消耗]。消耗 X 点鬼气。接下来 X 张牌按费用获得强化：0-1 费→打出时 +1 能量；2 费→+1 力 +1 敏 抽 2；3 费→一张 0 费复制入弃牌堆。', 'Haunt', '[Exhaust]. Spend X Ghost Qi. Your next X cards gain a bonus by cost: 0-1 cost → +1 Energy; 2 cost → +1 Str +1 Dex, draw 2; 3 cost → a 0-cost copy to discard.'),

    # ---- Common 攻击 ----
    'MoZhan': ('墨斩', '造成 {Damage:diff()} 点伤害。本回合已获鬼气则改为 {Conditional:diff()} 点。', 'Ink Slash', 'Deal {Damage:diff()} damage. If you gained Ghost Qi this turn, deal {Conditional:diff()} instead.'),
    'PoFeng': ('破风', '造成 {Damage:diff()} 点伤害，获得 {GhostQiGain:diff()} 点鬼气。', 'Windbreak', 'Deal {Damage:diff()} damage. Gain {GhostQiGain:diff()} Ghost Qi.'),
    'GuiYa': ('鬼压', '造成 {Damage:diff()} 点伤害。若目标已虚弱，改为 {Conditional:diff()} 点。', 'Ghost Press', 'Deal {Damage:diff()} damage. If the target is Weak, deal {Conditional:diff()} instead.'),
    'SiGuang': ('撕光', '造成 {Damage:diff()} 点伤害。你下一张攻击牌 +{Bonus:diff()} 伤害。', 'Tear Light', 'Deal {Damage:diff()} damage. Your next Attack deals +{Bonus:diff()} damage.'),
    'MoHen': ('墨痕', '对所有敌人造成 {Damage:diff()} 点伤害，给予 {Vulnerable:diff()} 层易伤。', 'Ink Scar', 'Deal {Damage:diff()} damage to ALL enemies. Apply {Vulnerable:diff()} Vulnerable.'),
    'ChanYing': ('缠影', '造成 {Damage:diff()} 点伤害，给予 {Weak:diff()} 层虚弱。', 'Binding Shadow', 'Deal {Damage:diff()} damage. Apply {Weak:diff()} Weak.'),
    'YinXi': ('隐袭', '造成 {Damage:diff()} 点伤害，抽 {Cards:diff()} 张牌。', 'Hidden Strike', 'Deal {Damage:diff()} damage. Draw {Cards:diff()} cards.'),
    'LianMo': ('连墨', '造成 {Damage:diff()} 点伤害，共 {Repeat:diff()} 次。', 'Chained Ink', 'Deal {Damage:diff()} damage {Repeat:diff()} times.'),

    # ---- Common 技能 ----
    'MoDun': ('墨盾', '获得 {Block:diff()} 点格挡。本回合已打作祟则额外 +{Bonus:diff()} 格挡。', 'Ink Shield', 'Gain {Block:diff()} Block. If you played Haunt this turn, gain +{Bonus:diff()} more.'),
    'JuQi': ('聚气', '获得 {Block:diff()} 点格挡，获得 {GhostQiGain:diff()} 点鬼气。', 'Gather Qi', 'Gain {Block:diff()} Block. Gain {GhostQiGain:diff()} Ghost Qi.'),
    'HuiMo': ('回墨', '抽 {Cards:diff()} 张牌。本回合已打出 2 张以上牌则再抽 1 张。', 'Ink Return', 'Draw {Cards:diff()} cards. If you played 2+ cards this turn, draw 1 more.'),
    'ChengXi': ('澄息', '获得 {Block:diff()} 点格挡，抽 {Cards:diff()} 张牌。', 'Clear Breath', 'Gain {Block:diff()} Block. Draw {Cards:diff()} cards.'),
    'YinQi': ('引气', '获得 {GhostQiGain:diff()} 点鬼气，抽 {Cards:diff()} 张牌。', 'Draw Qi', 'Gain {GhostQiGain:diff()} Ghost Qi. Draw {Cards:diff()} cards.'),
    'GuiBi': ('鬼壁', '获得 {Block:diff()} 点格挡。本回合已打作祟则对所有敌人施加 {Weak:diff()} 层虚弱。', 'Ghost Wall', 'Gain {Block:diff()} Block. If you played Haunt this turn, apply {Weak:diff()} Weak to ALL enemies.'),
    'MoYun': ('墨涌', '获得 {GhostQiGain:diff()} 点鬼气，抽 {Cards:diff()} 张牌，丢弃 1 张牌。', 'Ink Surge', 'Gain {GhostQiGain:diff()} Ghost Qi. Draw {Cards:diff()} cards. Discard 1 card.'),
    'MoYi': ('墨移', '你下一张牌费用 -{CostReduction:diff()}，抽 1 张牌。', 'Ink Shift', 'Your next card costs {CostReduction:diff()} less. Draw 1 card.'),

    # ---- Common 能力 ----
    'GuiMoJia': ('鬼墨甲', '每回合开始获得 {Block:diff()} 点格挡。', 'Ghost Ink Armor', 'At the start of each turn, gain {Block:diff()} Block.'),
    'LingLi': ('凌厉', '每回合你第 1 张攻击牌伤害 +{Bonus:diff()}。', 'Fierce', 'Your first Attack each turn deals +{Bonus:diff()} damage.'),
    'XinMo': ('心墨', '每打出 1 张 0 费牌，获得 1 点鬼气（每回合上限 {Cap:diff()}）。', 'Heart Ink', 'Whenever you play a 0-cost card, gain 1 Ghost Qi (up to {Cap:diff()} per turn).'),
    'ZhanYi': ('战意', '获得 {Strength:diff()} 点力量。', 'Battle Will', 'Gain {Strength:diff()} Strength.'),

    # ---- Uncommon 攻击 ----
    'MoLong': ('墨龙', '造成 {Damage:diff()} 点伤害。本回合每打出 1 张牌，此伤害 +{Bonus:diff()}。', 'Ink Dragon', 'Deal {Damage:diff()} damage. For each card played this turn, +{Bonus:diff()} damage.'),
    'SuiFeng': ('随风', '造成 {Damage:diff()} 点伤害，给予 {Vulnerable:diff()} 层易伤。', 'With the Wind', 'Deal {Damage:diff()} damage. Apply {Vulnerable:diff()} Vulnerable.'),
    'LianZhan': ('连斩', '造成 {Damage:diff()} 点伤害，共 {Repeat:diff()} 次。若击杀敌人，再造成 2 倍伤害。', 'Chain Slash', 'Deal {Damage:diff()} damage {Repeat:diff()} times. If you kill, deal double.'),
    'HanMo': ('寒墨', '对所有敌人造成 {Damage:diff()} 点伤害，给予 {Weak:diff()} 层虚弱与 {Vulnerable:diff()} 层易伤。', 'Cold Ink', 'Deal {Damage:diff()} to ALL. Apply {Weak:diff()} Weak and {Vulnerable:diff()} Vulnerable.'),
    'MoSi': ('墨丝', '造成 {Damage:diff()} 点伤害，获得 {GhostQiGain:diff()} 点鬼气，抽 1 张牌。', 'Ink Thread', 'Deal {Damage:diff()} damage. Gain {GhostQiGain:diff()} Ghost Qi. Draw 1.'),
    'ChiXue': ('赤血', '失去 {BloodCost:diff()} 点生命，造成 {Damage:diff()} 点伤害。', 'Red Blood', 'Lose {BloodCost:diff()} HP. Deal {Damage:diff()} damage.'),
    'YingDian': ('影点', '造成 1 点伤害，重复等于当前鬼气次（上限 {Cap:diff()}）。', 'Shadow Dot', 'Deal 1 damage repeated up to {Cap:diff()} times (equal to Ghost Qi).'),
    'GuiLie': ('鬼裂', '造成 {Damage:diff()} 点伤害，给予 {Vulnerable:diff()} 层易伤。', 'Ghost Rift', 'Deal {Damage:diff()} damage. Apply {Vulnerable:diff()} Vulnerable.'),
    'MoChao': ('墨潮', '对所有敌人造成 {Damage:diff()} 点伤害。本回合已打作祟则额外 +{Bonus:diff()}。', 'Ink Tide', 'Deal {Damage:diff()} to ALL. If you played Haunt this turn, +{Bonus:diff()}.'),
    'PoKong': ('破空', '造成 {Damage:diff()} 点伤害。本回合已打作祟则改为 {Conditional:diff()} 点。', 'Break Void', 'Deal {Damage:diff()} damage. If you played Haunt this turn, {Conditional:diff()} instead.'),
    'XuShi': ('虚实', '造成 {Damage:diff()} 点伤害，获得 {Block:diff()} 点格挡。', 'Void and Solid', 'Deal {Damage:diff()} damage. Gain {Block:diff()} Block.'),
    'ChiGui': ('赤鬼', '造成 {Damage:diff()} 点伤害。若目标已易伤，伤害翻倍。', 'Red Ghost', 'Deal {Damage:diff()} damage. If the target is Vulnerable, double the damage.'),

    # ---- Uncommon 技能 ----
    'YinBi': ('隐壁', '获得 {Block:diff()} 点格挡，抽 {Cards:diff()} 张牌。', 'Hidden Wall', 'Gain {Block:diff()} Block. Draw {Cards:diff()} cards.'),
    'MoQi': ('墨气', '获得 {GhostQiGain:diff()} 点鬼气，获得 {Block:diff()} 点格挡。', 'Ink Qi', 'Gain {GhostQiGain:diff()} Ghost Qi. Gain {Block:diff()} Block.'),
    'HuiHun': ('回魂', '[消耗]。抽 {Cards:diff()} 张牌，获得 {GhostQiGain:diff()} 点鬼气。', 'Return Soul', '[Exhaust]. Draw {Cards:diff()} cards. Gain {GhostQiGain:diff()} Ghost Qi.'),
    'NingMo': ('凝墨', '获得 {Block:diff()} 点格挡，获得 {GhostQiGain:diff()} 点鬼气。', 'Condensed Ink', 'Gain {Block:diff()} Block. Gain {GhostQiGain:diff()} Ghost Qi.'),
    'TuoYing': ('脱影', '抽 {Cards:diff()} 张牌，丢弃 1 张牌。', 'Shed Shadow', 'Draw {Cards:diff()} cards. Discard 1 card.'),
    'AnXiang': ('暗香', '获得 {Block:diff()} 点格挡，对所有敌人施加 {Weak:diff()} 层虚弱。', 'Dark Fragrance', 'Gain {Block:diff()} Block. Apply {Weak:diff()} Weak to ALL enemies.'),
    'MoHua': ('墨花', '获得 {GhostQiGain:diff()} 点鬼气，抽 {Cards:diff()} 张牌。', 'Ink Flower', 'Gain {GhostQiGain:diff()} Ghost Qi. Draw {Cards:diff()} cards.'),
    'XunFeng': ('巡风', '获得 {Block:diff()} 点格挡。本回合已打作祟则本回合所有攻击 +{Bonus:diff()} 伤害。', 'Patrol Wind', 'Gain {Block:diff()} Block. If you played Haunt this turn, all Attacks this turn +{Bonus:diff()} damage.'),
    'MingJing': ('明镜', '复制手牌中 1 张牌（复制品 -1 费）入弃牌堆。', 'Clear Mirror', 'Copy a card in hand (copy costs 1 less) to your discard pile.'),
    'JieYin': ('借阴', '本回合获得 {Strength:diff()} 点力量与 {Dexterity:diff()} 点敏捷。', 'Borrow Shade', 'Gain {Strength:diff()} Strength and {Dexterity:diff()} Dexterity this turn.'),
    'GuiZhao': ('鬼爪', '造成 {Damage:diff()} 点伤害，给予 {Vulnerable:diff()} 层易伤，抽 1 张牌。', 'Ghost Claw', 'Deal {Damage:diff()} damage. Apply {Vulnerable:diff()} Vulnerable. Draw 1.'),
    'MoWu': ('墨舞', '获得 {Block:diff()} 点格挡，抽 2 张牌。', 'Ink Dance', 'Gain {Block:diff()} Block. Draw 2 cards.'),
    'HuiMoLian': ('回墨链', '[消耗]。本回合每打出 1 张 0 费牌，抽 {Cards:diff()} 张牌。', 'Ink Chain', '[Exhaust]. Whenever you play a 0-cost card this turn, draw {Cards:diff()}.'),

    # ---- Uncommon 能力 ----
    'GuiMoXin': ('鬼墨心', '每回合开始获得 {GhostQiPerTurn:diff()} 点鬼气。', 'Ghost Ink Heart', 'At the start of each turn, gain {GhostQiPerTurn:diff()} Ghost Qi.'),
    'DianZeng': ('点增', '你的作祟点档（0-1 费）额外 +{ExtraEnergy:diff()} 能量。', 'Dot Boost', 'Your Haunt Dot tier (0-1 cost) grants +{ExtraEnergy:diff()} Energy.'),
    'XianZeng': ('线增', '你的作祟线档（2 费）额外 +{ExtraBlock:diff()} 格挡。', 'Line Boost', 'Your Haunt Line tier (2 cost) grants +{ExtraBlock:diff()} Block.'),
    'MianZeng': ('面增', '你的作祟面档（3+ 费）复制 {CopyCount:diff()} 张而非 1 张。', 'Face Boost', 'Your Haunt Face tier (3+ cost) copies {CopyCount:diff()} instead of 1.'),
    'ZuoSuiZhang': ('作祟掌', '你的作祟影响张数 +{ExtraCount:diff()}。', 'Haunt Palm', 'Your Haunt affects +{ExtraCount:diff()} cards.'),
    'GuiXi': ('鬼息', '每当你获得鬼气，获得 {Block:diff()} 点格挡。', 'Ghost Breath', 'Whenever you gain Ghost Qi, gain {Block:diff()} Block.'),
    'MoYin': ('墨瘾', '每当你打出作祟，抽 {Cards:diff()} 张牌。', 'Ink Addiction', 'Whenever you play Haunt, draw {Cards:diff()} cards.'),
    'LingHun': ('灵魂', '每回合你第 1 张技能牌费用 -1。', 'Soul', 'Your first Skill each turn costs 1 less.'),
    'XuShiJie': ('虚实界', '你的攻击牌命中时额外造成 {Pierce:diff()} 点无法格挡伤害。', 'Void Realm', 'Your Attacks deal {Pierce:diff()} extra unblockable damage.'),
    'GuiYing': ('鬼影', '每打出 1 张 2 费牌，获得 1 点鬼气（每回合上限 {Cap:diff()}）。', 'Ghost Shadow', 'Whenever you play a 2-cost card, gain 1 Ghost Qi (up to {Cap:diff()} per turn).'),
    'CanYing': ('残影', '回合结束若本回合没打作祟，获得 {GhostQi:diff()} 点鬼气。', 'Remnant', 'At end of turn, if you did not play Haunt, gain {GhostQi:diff()} Ghost Qi.'),
    'MoXin': ('墨心', '你打出的 0 费牌伤害与格挡 +{Bonus:diff()}。', 'Ink Core', 'Your 0-cost cards deal +{Bonus:diff()} damage and Block.'),

    # ---- Rare 攻击 ----
    'WanJieZhan': ('万界斩', '[消耗]。造成 {Damage:diff()} 点伤害。', 'Myriad Slash', '[Exhaust]. Deal {Damage:diff()} damage.'),
    'MoRanJiangShan': ('墨染江山', '对所有敌人造成 {Damage:diff()} 点伤害，获得 {GhostQiGain:diff()} 点鬼气。', 'Ink Stained Realm', 'Deal {Damage:diff()} to ALL. Gain {GhostQiGain:diff()} Ghost Qi.'),
    'PoXiao': ('破晓', '造成 {Damage:diff()} 点伤害。若目标已易伤，伤害翻倍。', 'Dawnbreak', 'Deal {Damage:diff()} damage. If the target is Vulnerable, double the damage.'),
    'LingXiZhan': ('灵犀斩', '造成 {Damage:diff()} 点伤害，抽 {Cards:diff()} 张牌，获得 1 点鬼气。', 'Spirit Slash', 'Deal {Damage:diff()} damage. Draw {Cards:diff()} cards. Gain 1 Ghost Qi.'),
    'XueGui': ('血鬼', '失去 {BloodCost:diff()} 点生命，造成 {Damage:diff()} 点伤害。', 'Blood Ghost', 'Lose {BloodCost:diff()} HP. Deal {Damage:diff()} damage.'),
    'MoHai': ('墨海', '对所有敌人造成 {Damage:diff()} 点伤害，给予 {Vulnerable:diff()} 层易伤。', 'Ink Sea', 'Deal {Damage:diff()} to ALL. Apply {Vulnerable:diff()} Vulnerable.'),
    'GuiShen': ('鬼神', '造成 {Damage:diff()} 点伤害，共 {Repeat:diff()} 次。', 'Ghost God', 'Deal {Damage:diff()} damage {Repeat:diff()} times.'),
    'MoJie': ('墨劫', '造成 {Damage:diff()} 点伤害，本回合你所有后续攻击 +{Bonus:diff()} 伤害。', 'Ink Calamity', 'Deal {Damage:diff()} damage. Your Attacks this turn deal +{Bonus:diff()}.'),

    # ---- Rare 技能 ----
    'WanJieBi': ('万界壁', '获得 {Block:diff()} 点格挡，抽 {Cards:diff()} 张牌。', 'Myriad Wall', 'Gain {Block:diff()} Block. Draw {Cards:diff()} cards.'),
    'XuShen': ('虚神', '获得 {Block:diff()} 点格挡，本回合你所有伤害 +{Bonus:diff()}。', 'Void God', 'Gain {Block:diff()} Block. Your damage this turn +{Bonus:diff()}.'),
    'MoYuan': ('墨渊', '获得 {GhostQiGain:diff()} 点鬼气，抽 {Cards:diff()} 张牌，丢弃 1 张牌。', 'Ink Abyss', 'Gain {GhostQiGain:diff()} Ghost Qi. Draw {Cards:diff()} cards. Discard 1.'),
    'LingXi': ('灵犀', '[消耗]。你下一张牌费用 -{CostReduction:diff()}。', 'Spirit Link', '[Exhaust]. Your next card costs {CostReduction:diff()} less.'),
    'HuiTian': ('回天', '[消耗]。将本场战斗已消耗的 {Return:diff()} 张牌返回手牌。', 'Reverse Heaven', '[Exhaust]. Return {Return:diff()} exhausted card(s) to your hand.'),
    'MoWuJue': ('墨舞·绝', '获得 {Block:diff()} 点格挡，抽 3 张牌。', 'Ink Dance Finale', 'Gain {Block:diff()} Block. Draw 3 cards.'),
    'WanNian': ('万年', '[消耗]。获得 {GhostQiGain:diff()} 点鬼气，抽 3 张牌。', 'Ten Thousand Years', '[Exhaust]. Gain {GhostQiGain:diff()} Ghost Qi. Draw 3 cards.'),
    'LingXu': ('灵虚', '本回合你所有 0 费牌可免费再打出一次（复制入弃牌堆）。', 'Spirit Void', 'This turn, all your 0-cost cards can be played again for free (copy to discard).'),
    'MoSheng': ('墨生', '本回合你每打出 1 张牌，获得 {Block:diff()} 点格挡。', 'Ink Life', 'Whenever you play a card this turn, gain {Block:diff()} Block.'),
    'ZuoSuiYin': ('作祟引', '抽 {Cards:diff()} 张牌，获得 1 点鬼气，本回合作祟费用 -{CostReduction:diff()} 鬼气。', 'Haunt Lure', 'Draw {Cards:diff()} cards. Gain 1 Ghost Qi. Haunt costs {CostReduction:diff()} less this turn.'),

    # ---- Rare 能力 ----
    'MoShen': ('墨神', '每回合开始获得 1 点能量。', 'Ink God', 'At the start of each turn, gain 1 Energy.'),
    'GuiLuo': ('鬼罗', '你的作祟影响张数翻倍。', 'Ghost Net', 'Your Haunt affects twice as many cards.'),
    'WanJieLin': ('万界林', '你的作祟三档全强化：点档 +{PointEnergy:diff()} 能量、线档 +{LineStat:diff()} 力 +{LineStat:diff()} 敏、面档复制 {FaceCopy:diff()} 张。', 'Myriad Forest', 'Your Haunt is fully enhanced: Dot +{PointEnergy:diff()} Energy, Line +{LineStat:diff()} Str/Dex, Face copies {FaceCopy:diff()}.'),
    'XuKongJie': ('虚空界', '每回合你打出的第 1 张牌，其打出后返回手牌（每张牌限 1 次）。', 'Void Boundary', 'The first card you play each turn returns to your hand (once per card).'),
    'MoLing': ('墨灵', '每当你打出作祟，获得 1 点力量（每回合上限 {Cap:diff()}）。', 'Ink Spirit', 'Whenever you play Haunt, gain 1 Strength (up to {Cap:diff()} per turn).'),
    'GuiPo': ('鬼魄', '每场战斗开始，获得 {GhostQi:diff()} 点鬼气。', 'Ghost Soul', 'At the start of combat, gain {GhostQi:diff()} Ghost Qi.'),
    'HunGui': ('魂归', '每当你的牌被作祟强化，额外抽 {Draw:diff()} 张牌。', 'Soul Return', 'Whenever your card is enhanced by Haunt, draw {Draw:diff()} extra.'),
    'ZuoSuiJing': ('作祟精', '你的作祟费用减半（向下取整）。', 'Haunt Essence', 'Your Haunt costs half (rounded down).'),

    # ---- Ancient ----
    'GuiMoZhenYan': ('鬼墨真言', '每回合开始获得 {GhostQiPerTurn:diff()} 点鬼气，你的作祟不再[消耗]。', 'Ghost Ink Truth', 'At the start of each turn, gain {GhostQiPerTurn:diff()} Ghost Qi. Your Haunt no longer Exhausts.'),
    'XianGuZhiZhao': ('先古之兆', '[消耗]。获得 {GhostQiGain:diff()} 点鬼气，抽 {Draw:diff()} 张牌。', 'Ancient Omen', '[Exhaust]. Gain {GhostQiGain:diff()} Ghost Qi. Draw {Draw:diff()} cards.'),
    'MoYu': ('墨域', '[消耗]。对所有敌人造成 {Damage:diff()} 点伤害，本回合已打作祟则全体 +{Conditional:diff()}。', 'Ink Domain', '[Exhaust]. Deal {Damage:diff()} to ALL. If you played Haunt this turn, +{Conditional:diff()}.'),
    'WanJieLaiChao': ('万界来朝', '每场战斗开始获得 {GhostQiGain:diff()} 点鬼气，抽 {Draw:diff()} 张牌。', 'Myriad Tribute', 'At the start of combat, gain {GhostQiGain:diff()} Ghost Qi. Draw {Draw:diff()} cards.'),
}


# ============================================================================
# 先古选项文案（表 cards，键 = <CARD>_OPTION.description，★ 不含任何 {占位符}）
# 事件界面拿不到卡牌 DynamicVars，所以这里写死纯文本，别放 {Var}。
# ============================================================================
ANCIENT_OPTION_ZH = {
    PFX + 'CARD_GUI_MO_ZHEN_YAN_OPTION.description':
        '获得一张「鬼墨真言」：每回合获得鬼气，且作祟不再消耗。',
    PFX + 'CARD_XIAN_GU_ZHI_ZHAO_OPTION.description':
        '获得一张「先古之兆」：消耗，获得大量鬼气并抽牌。',
    PFX + 'CARD_MO_YU_OPTION.description':
        '获得一张「墨域」：对全体造成伤害，打过作祟则更痛。',
    PFX + 'CARD_WAN_JIE_LAI_CHAO_OPTION.description':
        '获得一张「万界来朝」：每场战斗开始获得鬼气并抽牌。',
}
ANCIENT_OPTION_EN = {
    PFX + 'CARD_GUI_MO_ZHEN_YAN_OPTION.description':
        'Obtain "Ghost Ink Truth": gain Ghost Qi each turn, and Haunt no longer Exhausts.',
    PFX + 'CARD_XIAN_GU_ZHI_ZHAO_OPTION.description':
        'Obtain "Ancient Omen": Exhaust, gain lots of Ghost Qi and draw.',
    PFX + 'CARD_MO_YU_OPTION.description':
        'Obtain "Ink Domain": damage ALL, stronger if you played Haunt.',
    PFX + 'CARD_WAN_JIE_LAI_CHAO_OPTION.description':
        'Obtain "Myriad Tribute": gain Ghost Qi and draw at combat start.',
}


# ============================================================================
# 能力（Power）本地化
# ============================================================================
POWERS = {
    # 作祟引擎与标记
    'ZuoSuiPower': ('作祟', '接下来 {Amount} 张牌按费用获得作祟强化。', 'Haunt', 'Your next {Amount} cards gain a Haunt bonus by cost.'),
    'ZuoSuiPlayedThisTurnPower': ('作祟标记', '本回合已打出作祟。', 'Haunt Mark', 'Haunt was played this turn.'),
    'GhostQiGainedThisTurnPower': ('鬼气标记', '本回合已获得鬼气。', 'Ghost Qi Mark', 'Ghost Qi was gained this turn.'),
    'CardsPlayedThisTurnPower': ('出牌计数', '本回合已打出 {Amount} 张牌。', 'Cards Played', '{Amount} cards played this turn.'),
    'WanJieRuLinCombatTrackerPower': ('战斗追踪', '追踪本回合出牌数。', 'Combat Tracker', 'Tracks cards played this turn.'),

    # 三档强化
    'DianZengPower': ('点增', '作祟点档（0-1 费）额外 +{ExtraEnergy} 能量。', 'Dot Boost', 'Haunt Dot tier grants +{ExtraEnergy} extra Energy.'),
    'XianZengPower': ('线增', '作祟线档（2 费）额外 +{ExtraBlock} 格挡。', 'Line Boost', 'Haunt Line tier grants +{ExtraBlock} extra Block.'),
    'MianZengPower': ('面增', '作祟面档（3+ 费）复制 {CopyCount} 张。', 'Face Boost', 'Haunt Face tier copies {CopyCount}.'),
    'WanJieLinPower': ('万界林', '作祟三档全强化。', 'Myriad Forest', 'Haunt is fully enhanced.'),

    # 张数/费用/去消耗
    'ZuoSuiZhangPower': ('作祟掌', '作祟影响张数 +{ExtraCount}。', 'Haunt Palm', 'Haunt affects +{ExtraCount} cards.'),
    'GuiLuoPower': ('鬼罗', '作祟影响张数翻倍。', 'Ghost Net', 'Haunt affects twice as many cards.'),
    'ZuoSuiJingPower': ('作祟精', '作祟费用减免。', 'Haunt Essence', 'Haunt cost reduced.'),
    'GuiMoZhenYanPower': ('鬼墨真言', '每回合 +{GhostQiPerTurn} 鬼气，作祟不再[消耗]。', 'Ghost Ink Truth', 'Each turn +{GhostQiPerTurn} Ghost Qi. Haunt no longer Exhausts.'),

    # 鬼气经济
    'GuiMoXinPower': ('鬼墨心', '每回合开始 +{GhostQiPerTurn} 鬼气。', 'Ghost Ink Heart', 'At the start of each turn, gain {GhostQiPerTurn} Ghost Qi.'),
    'GuiPoPower': ('鬼魄', '每场战斗开始 +{GhostQi} 鬼气。', 'Ghost Soul', 'At the start of combat, gain {GhostQi} Ghost Qi.'),
    'XinMoPower': ('心墨', '每打 0 费牌 +1 鬼气（每回合上限 {Cap}）。', 'Heart Ink', 'Each 0-cost card +1 Ghost Qi (up to {Cap} per turn).'),
    'GuiYingPower': ('鬼影', '每打 2 费牌 +1 鬼气（每回合上限 {Cap}）。', 'Ghost Shadow', 'Each 2-cost card +1 Ghost Qi (up to {Cap} per turn).'),
    'CanYingPower': ('残影', '回合结束若没打作祟 +{GhostQi} 鬼气。', 'Remnant', 'End of turn: if no Haunt played, gain {GhostQi} Ghost Qi.'),
    'MoShenPower': ('墨神', '每回合开始 +1 能量。', 'Ink God', 'At the start of each turn, gain 1 Energy.'),
    'WanJieLaiChaoPower': ('万界来朝', '每场战斗开始 +{GhostQi} 鬼气，抽 {Draw} 张。', 'Myriad Tribute', 'At combat start, gain {GhostQi} Ghost Qi, draw {Draw}.'),

    # 作祟联动收益
    'MoYinPower': ('墨瘾', '每打作祟抽 {Cards} 张。', 'Ink Addiction', 'Each Haunt draws {Cards}.'),
    'MoLingPower': ('墨灵', '每打作祟 +1 力量（每回合上限 {Cap}）。', 'Ink Spirit', 'Each Haunt +1 Strength (up to {Cap} per turn).'),
    'HunGuiPower': ('魂归', '被作祟强化的牌额外抽 {Draw} 张。', 'Soul Return', 'Cards enhanced by Haunt draw {Draw} extra.'),
    'GuiXiPower': ('鬼息', '每获鬼气 +{Block} 格挡。', 'Ghost Breath', 'Each Ghost Qi gained gives +{Block} Block.'),
    'HuiMoLianPower': ('回墨链', '本回合每打 0 费牌抽 {Cards} 张。', 'Ink Chain', 'Each 0-cost card this turn draws {Cards}.'),

    # 回合/费用/状态
    'LingHunPower': ('灵魂', '每回合第 1 张技能牌 -1 费。', 'Soul', 'Your first Skill each turn costs 1 less.'),
    'XuShiJiePower': ('虚实界', '攻击牌命中时额外 {Pierce} 点无法格挡伤害。', 'Void Realm', 'Attacks deal {Pierce} extra unblockable damage.'),
    'MoXinPower': ('墨心', '0 费牌伤害与格挡 +{Bonus}。', 'Ink Core', '0-cost cards +{Bonus} damage and Block.'),
    'XuKongJiePower': ('虚空界', '每回合第 1 张牌打出后返回手牌。', 'Void Boundary', 'First card each turn returns to hand.'),
    'MoShengPower': ('墨生', '每打 1 张牌 +{BlockPerCard} 格挡。', 'Ink Life', 'Each card played gives +{BlockPerCard} Block.'),
    'GuiMoJiaPower': ('鬼墨甲', '每回合开始 +{Block} 格挡。', 'Ghost Ink Armor', 'At turn start, gain {Block} Block.'),
    'LingLiPower': ('凌厉', '每回合第 1 张攻击 +{Bonus} 伤害。', 'Fierce', 'First Attack each turn +{Bonus} damage.'),
    'MoYiNextCardPower': ('墨移', '下一张牌费用 -{CostReduction}。', 'Ink Shift', 'Next card costs {CostReduction} less.'),
    'LingXuPower': ('灵虚', '0 费牌免费再打一次。', 'Spirit Void', '0-cost cards can be played again free.'),

    # 临时属性（作祟线档用）
    'WanJieTempStrengthPower': ('临时力量', '本回合力量 +{Amount}（回合结束消失）。', 'Temporary Strength', 'Gain +{Amount} Strength this turn (removed at end of turn).'),
    'WanJieTempDexterityPower': ('临时敏捷', '本回合敏捷 +{Amount}（回合结束消失）。', 'Temporary Dexterity', 'Gain +{Amount} Dexterity this turn (removed at end of turn).'),
}


RELIC_ZH = {
    PFX + 'RELIC_GUI_MO.title': '鬼墨',
    PFX + 'RELIC_GUI_MO.description':
        '每场战斗开始，获得 1 点鬼气。',
    PFX + 'RELIC_JIN_SI_GUI_MO.title': '金丝鬼墨',
    PFX + 'RELIC_JIN_SI_GUI_MO.description':
        '每场战斗开始，获得 2 点鬼气。',
}

RELIC_EN = {
    PFX + 'RELIC_GUI_MO.title': 'Ghost Ink',
    PFX + 'RELIC_GUI_MO.description':
        'At the start of each combat, gain 1 Ghost Qi.',
    PFX + 'RELIC_JIN_SI_GUI_MO.title': 'Gold-Thread Ghost Ink',
    PFX + 'RELIC_JIN_SI_GUI_MO.description':
        'At the start of each combat, gain 2 Ghost Qi.',
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
    PFX + 'GHOST_QI_TITLE': '鬼气',
    PFX + 'GHOST_QI_DESC': '本模组的特殊能量。战斗内靠卡牌与能力获得，专供「作祟」消耗。',
}
TIPS_EN = {
    PFX + 'GHOST_QI_TITLE': 'Ghost Qi',
    PFX + 'GHOST_QI_DESC': 'This mod\'s special energy. Gained in combat via cards and powers, spent only by "Haunt".',
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
    for cls, v in CARDS.items():
        k = PFX + 'CARD_' + snake(cls) + '.'
        cards[k + 'title'] = v[0] if zh else v[2]
        cards[k + 'description'] = v[1] if zh else v[3]
        cards[k + 'smartDescription'] = v[1] if zh else v[3]
    # 先古选项文案（追加进 cards 表）
    cards.update(ANCIENT_OPTION_ZH if zh else ANCIENT_OPTION_EN)
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
