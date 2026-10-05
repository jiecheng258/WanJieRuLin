# -*- coding: utf-8 -*-
r"""★ v0.5 卡牌生成器 —— 「点 · 线 · 面」框架（唯一真相源）。

与原版对齐的规模（来源：官方 wiki 的 Ironclad 词条）：
    常规 80 = 普通 20 / 罕见 35 / 稀有 25
  + 初始 5（打击 / 防御 / 起笔 / 运笔 / 成幅）
  + 派生 5（先古 2 / 事件 3）
  = 90 张

三类归属由 aspect= 参数声明，发射成 `public override WanJieAspect Aspect => ...`。

三机制（已实现于 Powers/）：
    笔锋 BiFengPower  点牌产出 → 下一张牌费用 −层数
    力道 LiDaoPower   线牌产出 → 本回合伤害/格挡 +层数
    墨韵 MoYunPower   面牌产出 → 跨回合累积；每满 5 层削弱点/线牌
"""
import io, os

OUT = r'C:\Users\wangx\Documents\Default Project\WanJieRuLin\WanJieRuLinCode\Cards'
HDR = '''using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Aspects;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;
'''

TC = '        ArgumentNullException.ThrowIfNull(cardPlay.Target);\n'


def dmg(a, target='cardPlay.Target', hits=None):
    return '        await DealDamage(choiceContext, %s, %s%s);\n' % (target, a, (', %s' % hits) if hits else '')


def dmg_all(a):
    return '        await DealDamageToAll(choiceContext, %s);\n' % a


def atk(a, hits=None, target='cardPlay.Target'):
    s = ['        await DamageCmd.Attack(%s)\n' % a,
         '            .FromCard(this, cardPlay)\n',
         '            .Targeting(%s)\n' % target]
    if hits:
        s.append('            .WithHitCount(%s)\n' % hits)
    s.append('            .Execute(choiceContext);\n')
    return ''.join(s)


def block(a):
    return '        await GainBlock(choiceContext, %s);\n' % a


def draw(n):
    return '        await Draw(choiceContext, %s);\n' % n


def energy(n):
    return '        await GainEnergy(%s);\n' % n


def pw(cls, n='1m', who='self'):
    if who == 'self':
        return '        await ApplySelf<%s>(choiceContext, %s);\n' % (cls, n)
    return '        await ApplyTo<%s>(choiceContext, cardPlay.Target, %s);\n' % (cls, n)


def getpw(cls, sets):
    s = ['        var power = await ApplySelfAndGet<%s>(choiceContext, 1m);\n' % cls,
         '        if (power is not null)\n        {\n']
    for f, v, d in sets:
        s.append('            power.%s = DynamicVars.GetIntOrDefault("%s", %s);\n' % (f, v, d))
    s.append('        }\n')
    return ''.join(s)


DV = 'new DamageVar(%sm, ValueProp.Move)'
BV = 'new BlockVar(%sm, ValueProp.Move)'


def vi(n, v):
    return 'ModCardVars.Int("%s", %s)' % (n, v)


def vc(n):
    return 'ModCardVars.Cards(%s)' % n


def vr(n):
    return 'ModCardVars.Repeat(%s)' % n


SPEC = []


def card(cls, cn, cost, ctype, rarity, target, aspect=None, ex=False, cond=None,
         vars_=(), play=(), upg=(), doc='', archaic=None, starter=None, gains_block=False):
    SPEC.append(dict(cls=cls, cn=cn, cost=cost, ctype=ctype, rarity=rarity, target=target,
                     aspect=aspect, ex=ex, cond=cond, vars=list(vars_), play=list(play),
                     upg=list(upg), doc=doc, archaic=archaic, starter=starter,
                     gains_block=gains_block))


# ============================================================================
# 初始 ×5
# ============================================================================
card('DaJi', '打击', 1, 'Attack', 'Basic', 'AnyEnemy', starter=4,
     vars_=[DV % 8], play=[TC, atk('DynamicVars.Damage.BaseValue')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(3m);'],
     doc='造成 8 点伤害。升级后 9 点。\n★ 与原版同档，不属于点线面。')

card('FangYu', '防御', 1, 'Skill', 'Basic', 'Self', starter=4, gains_block=True,
     vars_=[BV % 6], play=[block('DynamicVars.Block.BaseValue')],
     upg=['        DynamicVars.Block.UpgradeValueBy(3m);'],
     doc='获得 6 点格挡。升级后 8 点。\n★ 与原版同档，不属于点线面。')

card('QiBi', '起笔', 0, 'Skill', 'Basic', 'Self', aspect='Point', starter=1,
     archaic='WanJieRuLinAncient',
     vars_=[vi('Edge', 1), vc(1)],
     play=[draw('DynamicVars.Cards.IntValue'), pw('BiFengPower', 'DynamicVars.GetIntOrDefault("Edge", 1)')],
     upg=['        DynamicVars.Cards.UpgradeValueBy(1);'],
     doc='★ **点** —— 抽 1 张牌，获得 1 点[gold]笔锋[/gold]。\n'
         '升级 ★ 质变：抽 1 张。')

card('YunBi', '运笔', 1, 'Skill', 'Basic', 'Self', aspect='Line', starter=1, gains_block=True,
     vars_=[BV % 6, vi('Force', 1)],
     play=[block('DynamicVars.Block.BaseValue'),
           pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 1)')],
     upg=['        DynamicVars.Block.UpgradeValueBy(1m);'],
     doc='★ **线** —— 获得 6 点格挡，获得 1 点[gold]力道[/gold]。升级后 6 点格挡。')

card('ChengFu', '成幅', 2, 'Attack', 'Basic', 'AnyEnemy', aspect='Face',
     vars_=[DV % 16], play=[TC, atk('DynamicVars.Damage.BaseValue')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(4m);'],
     doc='★ **面** —— 造成 16 点伤害。升级后 17 点。')

# ============================================================================
# 普通 ×20（点 7 / 线 7 / 面 6）
# ============================================================================
# ---- 点 ×7 ----
card('QingDian', '轻点', 1, 'Attack', 'Common', 'AnyEnemy', aspect='Point',
     vars_=[DV % 6, vi('Edge', 1)],
     play=[TC, atk('DynamicVars.Damage.BaseValue'),
           pw('BiFengPower', 'DynamicVars.GetIntOrDefault("Edge", 1)')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(1m);'],
     doc='★ **点** —— 造成 6 点伤害，获得 1 点[gold]笔锋[/gold]。升级后 6 点伤害。')

card('DianRan', '点染', 1, 'Skill', 'Common', 'Self', aspect='Point', gains_block=True,
     vars_=[BV % 5, vi('Edge', 1)],
     play=[block('DynamicVars.Block.BaseValue'),
           pw('BiFengPower', 'DynamicVars.GetIntOrDefault("Edge", 1)')],
     upg=['        DynamicVars.Block.UpgradeValueBy(1m);'],
     doc='★ **点** —— 获得 5 点格挡，获得 1 点[gold]笔锋[/gold]。升级后 5 点格挡。')

card('WeiMang', '微芒', 0, 'Attack', 'Common', 'AnyEnemy', aspect='Point',
     vars_=[DV % 4], play=[TC, atk('DynamicVars.Damage.BaseValue')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(2m);'],
     doc='★ **点** —— 0 费，造成 4 点伤害。升级后 5 点。')

card('LuoDian', '落点', 0, 'Skill', 'Common', 'Self', aspect='Point',
     vars_=[vi('Edge', 2)],
     play=[pw('BiFengPower', 'DynamicVars.GetIntOrDefault("Edge", 2)')],
     upg=['        DynamicVars["Edge"].UpgradeValueBy(1);'],
     doc='★ **点** —— 0 费，获得 2 点[gold]笔锋[/gold]。升级后 3 点。')

card('ShuBi', '数笔', 1, 'Skill', 'Common', 'Self', aspect='Point',
     vars_=[vc(2)], play=[draw('DynamicVars.Cards.IntValue')],
     upg=['        DynamicVars.Cards.UpgradeValueBy(1);'],
     doc='★ **点** —— 抽 2 张牌。升级后抽 2 张。')

card('DianZhui', '点缀', 1, 'Attack', 'Common', 'AnyEnemy', aspect='Point',
     vars_=[DV % 5, vc(1)],
     play=[TC, atk('DynamicVars.Damage.BaseValue'), draw('DynamicVars.Cards.IntValue')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(2m);'],
     doc='★ **点** —— 造成 5 点伤害，抽 1 张牌。升级后 6 点伤害。')

card('DianPo', '点破', 1, 'Skill', 'Common', 'AnyEnemy', aspect='Point',
     vars_=[vi('StrengthLoss', 2), vi('Edge', 1)],
     play=[TC, '        await ApplyTo<StrengthPower>(choiceContext, cardPlay.Target, -DynamicVars.GetIntOrDefault("StrengthLoss", 1));\n',
           pw('BiFengPower', 'DynamicVars.GetIntOrDefault("Edge", 1)')],
     upg=['        DynamicVars["StrengthLoss"].UpgradeValueBy(1);'],
     doc='★ **点** —— 敌人失去 1 点[gold]力量[/gold]，获得 1 点[gold]笔锋[/gold]。升级后失 2 点力量。')

# ---- 线 ×7 ----
card('HengXian', '横线', 1, 'Attack', 'Common', 'AnyEnemy', aspect='Line',
     vars_=[DV % 9], play=[TC, atk('DynamicVars.Damage.BaseValue')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(3m);'],
     doc='★ **线** —— 造成 9 点伤害。升级后 10 点。')

card('ShuXian', '竖线', 1, 'Skill', 'Common', 'Self', aspect='Line', gains_block=True,
     vars_=[BV % 8], play=[block('DynamicVars.Block.BaseValue')],
     upg=['        DynamicVars.Block.UpgradeValueBy(3m);'],
     doc='★ **线** —— 获得 8 点格挡。升级后 9 点。')

card('GouXian', '勾线', 1, 'Skill', 'Common', 'Self', aspect='Line',
     vars_=[vi('Force', 2)],
     play=[pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 2)')],
     upg=['        DynamicVars["Force"].UpgradeValueBy(1);'],
     doc='★ **线** —— 获得 2 点[gold]力道[/gold]。升级后 3 点。')

card('LianXian', '连线', 1, 'Attack', 'Common', 'AnyEnemy', aspect='Line',
     vars_=[DV % 8, vi('Force', 1)],
     play=[TC, atk('DynamicVars.Damage.BaseValue'),
           pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 1)')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(2m);'],
     doc='★ **线** —— 造成 8 点伤害，获得 1 点[gold]力道[/gold]。升级后 8 点伤害。')

card('BiZhi', '笔直', 1, 'Skill', 'Common', 'Self', aspect='Line', gains_block=True,
     vars_=[BV % 9, vi('Force', 1)],
     play=[block('DynamicVars.Block.BaseValue'),
           pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 1)')],
     upg=['        DynamicVars.Block.UpgradeValueBy(2m);'],
     doc='★ **线** —— 获得 9 点格挡，获得 1 点[gold]力道[/gold]。升级后 9 点格挡。')

card('ZhiBi', '直笔', 1, 'Attack', 'Common', 'AnyEnemy', aspect='Line',
     vars_=[DV % 10], play=[TC, atk('DynamicVars.Damage.BaseValue')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(3m);'],
     doc='★ **线** —— 造成 10 点伤害。升级后 11 点。')

card('ChangXian', '长线', 1, 'Skill', 'Common', 'Self', aspect='Line',
     vars_=[vi('Force', 2), vc(1)],
     play=[draw('DynamicVars.Cards.IntValue'),
           pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 2)')],
     upg=['        DynamicVars["Force"].UpgradeValueBy(1);'],
     doc='★ **线** —— 抽 1 张牌，获得 1 点[gold]力道[/gold]。升级后 3 点力道。')

# ---- 面 ×6 ----
card('PoMian', '泼面', 2, 'Attack', 'Common', 'AnyEnemy', aspect='Face',
     vars_=[DV % 16], play=[TC, atk('DynamicVars.Damage.BaseValue')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(4m);'],
     doc='★ **面** —— 造成 16 点伤害。升级后 17 点。')

card('DaMian', '大面', 2, 'Skill', 'Common', 'Self', aspect='Face', gains_block=True,
     vars_=[BV % 13], play=[block('DynamicVars.Block.BaseValue')],
     upg=['        DynamicVars.Block.UpgradeValueBy(4m);'],
     doc='★ **面** —— 获得 13 点格挡。升级后 15 点。')

card('MoMian', '磨面', 2, 'Skill', 'Common', 'Self', aspect='Face',
     vars_=[vc(3)], play=[draw('DynamicVars.Cards.IntValue'), energy('1')],
     upg=['        DynamicVars.Cards.UpgradeValueBy(1);'],
     doc='★ **面** —— 抽 3 张牌，获得 3 点能量。升级后抽 4 张。')

card('ManFu', '满幅', 2, 'Attack', 'Common', 'AnyEnemy', aspect='Face',
     vars_=[BV % 10, DV % 12],
     play=[block('DynamicVars.Block.BaseValue'), TC, atk('DynamicVars.Damage.BaseValue')],
     upg=['        DynamicVars.Block.UpgradeValueBy(2m);',
          '        DynamicVars.Damage.UpgradeValueBy(3m);'],
     doc='★ **面** —— 获得 10 点格挡，造成 12 点伤害。\n升级 ★ 双升级：格挡 10 / 伤害 13。')

card('KuoMian', '阔面', 2, 'Attack', 'Common', 'AllEnemies', aspect='Face',
     vars_=[DV % 11], play=[dmg_all('DynamicVars.Damage.BaseValue')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(3m);'],
     doc='★ **面** —— 对**所有**敌人造成 11 点伤害。升级后 12 点。')

card('ChenMo', '沉墨', 2, 'Skill', 'Common', 'Self', aspect='Face',
     vars_=[vi('MoYun', 2)],
     play=[pw('MoYunPower', 'DynamicVars.GetIntOrDefault("MoYun", 2)')],
     upg=['        DynamicVars["MoYun"].UpgradeValueBy(1);'],
     doc='★ **面** —— 获得 2 点[gold]墨韵[/gold]（跨回合累积）。升级后 3 点。')



# ============================================================================
# 罕见 ×35（点 12 / 线 12 / 面 11）
# ============================================================================
# ---- 点 ×12 ----
card('DianJing', '点睛', 0, 'Skill', 'Uncommon', 'Self', aspect='Point',
     vars_=[vi('Edge', 2), vc(1)],
     play=[pw('BiFengPower', 'DynamicVars.GetIntOrDefault("Edge", 2)'),
           draw('DynamicVars.Cards.IntValue')],
     upg=['        DynamicVars["Edge"].UpgradeValueBy(1);'],
     doc='★ **点** —— 0 费：获得 1 点[gold]笔锋[/gold]，抽 1 张牌。升级后 3 点笔锋。')

card('DianShi', '点石', 1, 'Skill', 'Uncommon', 'Self', aspect='Point',
     vars_=[vi('Edge', 3)],
     play=[pw('BiFengPower', 'DynamicVars.GetIntOrDefault("Edge", 3)')],
     upg=['        DynamicVars["Edge"].UpgradeValueBy(1);'],
     doc='★ **点** —— 获得 3 点[gold]笔锋[/gold]。升级后 4 点。\n'
         '★ 一次把费用垫足，专门为「面」牌开路。')

card('YiDian', '一点', 0, 'Attack', 'Uncommon', 'AnyEnemy', aspect='Point',
     vars_=[DV % 3, vr(3)],
     play=[TC, atk('DynamicVars.Damage.BaseValue', hits='DynamicVars.Repeat.IntValue')],
     upg=['        DynamicVars.Repeat.UpgradeValueBy(1);'],
     doc='★ **点** —— 0 费：造成 3 点伤害，共 3 次。升级后 ★ 质变：4 次。')

card('LuoMo', '落墨', 1, 'Skill', 'Uncommon', 'Self', aspect='Point', gains_block=True,
     vars_=[BV % 6, vi('Edge', 1)],
     play=[block('DynamicVars.Block.BaseValue'),
           pw('BiFengPower', 'DynamicVars.GetIntOrDefault("Edge", 1)')],
     upg=['        DynamicVars.Block.UpgradeValueBy(2m);'],
     doc='★ **点** —— 获得 6 点格挡，获得 1 点[gold]笔锋[/gold]。升级后 7 点格挡。')

card('ShuSan', '疏散', 1, 'Skill', 'Uncommon', 'Self', aspect='Point',
     vars_=[vi('Discard', 1), vi('Edge', 3)],
     play=['        if (Owner is not { } p)\n        {\n            return;\n        }\n',
           '        var hand = CardPile.GetCards(p, [PileType.Hand]).ToList();\n',
           '        if (hand.Count == 0)\n        {\n            return;\n        }\n',
           '        var n = Math.Min(DynamicVars.GetIntOrDefault("Discard", 1), hand.Count);\n',
           '        var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, n);\n',
           '        var picked = (await CardSelectCmd.FromHandForDiscard(choiceContext, p, prefs, null, this)).ToList();\n',
           '        if (picked.Count > 0)\n        {\n            await CardCmd.Discard(choiceContext, picked);\n        }\n',
           '        await GainBiFeng(choiceContext, DynamicVars.GetIntOrDefault("Edge", 3));\n'],
     upg=['        DynamicVars["Edge"].UpgradeValueBy(1);'],
     doc='★ **点** —— 弃 1 张牌，获得 3 点[gold]笔锋[/gold]。升级后 4 点。\n'
         '★ 把打不出的牌换成费用优势。')

card('QianMiao', '浅描', 1, 'Attack', 'Uncommon', 'AnyEnemy', aspect='Point',
     vars_=[DV % 8, vi('EdgeBonus', 5)],
     play=[TC, atk('DynamicVars.Damage.BaseValue + (MyBiFeng > 0 ? DynamicVars.GetIntOrDefault("EdgeBonus", 4) : 0m)')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(1m);'],
     doc='★ **点** —— 造成 6 点伤害；若你持有[gold]笔锋[/gold]，额外造成 4 点。升级后 8 点。')

card('XiBi', '细笔', 0, 'Attack', 'Uncommon', 'AnyEnemy', aspect='Point',
     vars_=[DV % 5, vi('Edge', 1)],
     play=[TC, atk('DynamicVars.Damage.BaseValue'),
           pw('BiFengPower', 'DynamicVars.GetIntOrDefault("Edge", 1)')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(2m);'],
     doc='★ **点** —— 0 费：造成 5 点伤害，获得 1 点[gold]笔锋[/gold]。升级后 6 点伤害。')

card('DianYin', '点引', 1, 'Skill', 'Uncommon', 'Self', aspect='Point',
     vars_=[vc(2), vi('Edge', 1)],
     play=[draw('DynamicVars.Cards.IntValue'),
           pw('BiFengPower', 'DynamicVars.GetIntOrDefault("Edge", 1)')],
     upg=['        DynamicVars.Cards.UpgradeValueBy(1);'],
     doc='★ **点** —— 抽 2 张牌，获得 1 点[gold]笔锋[/gold]。升级后抽 2 张。')

card('MoDianV5', '墨点', 1, 'Attack', 'Uncommon', 'AnyEnemy', aspect='Point',
     vars_=[DV % 6], gains_block=True,
     play=[TC, atk('DynamicVars.Damage.BaseValue'), block('MyBiFeng')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(2m);'],
     doc='★ **点** —— 造成 6 点伤害；获得等同你[gold]笔锋[/gold]层数的格挡。升级后 7 点伤害。\n'
         '★ 笔锋在这张牌上是「一鱼两吃」：既减费又叠甲。')

card('DianPoXuKong', '点破虚空', 2, 'Attack', 'Uncommon', 'AnyEnemy', aspect='Point',
     vars_=[DV % 16], play=[TC, atk('DynamicVars.Damage.BaseValue')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(4m);'],
     doc='★ **点** —— 造成 16 点伤害。升级后 17 点。\n'
         '★ 面档的数值却算「点」—— 配合刀锋减费可以当低费重击用。')

card('HuiFeng', '回锋', 0, 'Attack', 'Uncommon', 'AnyEnemy', aspect='Point',
     vars_=[DV % 4, vi('BloodCost', 1)],
     play=['        if (Owner is { } self)\n        {\n',
           '            await CreatureCmd.Damage(choiceContext, self.Creature,\n',
           '                DynamicVars.GetIntOrDefault("BloodCost", 1), ValueProp.Move, self.Creature, null, null);\n',
           '        }\n', TC, atk('DynamicVars.Damage.BaseValue'),
           pw('HuiFengTracePower')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(2m);'],
     doc='★ **点 · 无限流一环** —— 0 费：造成 4 点伤害，**失去 1 点生命**。\n'
         '升级后 5 点伤害。\n'
         '★ 单独用很亏（自己掉血只换 3 点伤害）。它的价值在**联动**：\n'
         '   打出它会给「接笔」亮灯，接笔因此多产 1 点能量 —— 两张凑起来才开始成立。')

card('DianXianChengMian', '点线成面', 1, 'Skill', 'Uncommon', 'Self', aspect='Point',
     vars_=[vi('QiPerMoYun', 2)],
     play=['        var edge = await ClearBiFeng(choiceContext);\n',
           '        var per = Math.Max(1, DynamicVars.GetIntOrDefault("QiPerMoYun", 2));\n',
           '        var gain = edge / per;\n',
           '        if (gain > 0)\n        {\n',
           '            await MoYunPower.Gain(choiceContext, Owner!.Creature, gain);\n        }\n'],
     upg=['        DynamicVars["QiPerMoYun"].UpgradeValueBy(-1);'],
     doc='★ **点 → 面 的转换器** —— 消耗所有[gold]笔锋[/gold]，每 2 点换 1 点[gold]墨韵[/gold]。\n'
         '升级 ★ 质变：汇率 2:1 → 1:1。\n'
         '★ 均衡流的关键桥：笔锋这回合用不完，可以存成墨韵。')

# ---- 线 ×12 ----
card('LiTouZhiBei', '力透纸背', 1, 'Skill', 'Uncommon', 'Self', aspect='Line',
     vars_=[vi('Force', 3)],
     play=[pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 3)')],
     upg=['        DynamicVars["Force"].UpgradeValueBy(1);'],
     doc='★ **线** —— 获得 3 点[gold]力道[/gold]。升级后 4 点。')

card('YiBiHua', '一笔画', 2, 'Attack', 'Uncommon', 'AnyEnemy', aspect='Line',
     vars_=[DV % 10, vi('Force', 2)],
     play=[TC, atk('DynamicVars.Damage.BaseValue'),
           pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 2)')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(3m);'],
     doc='★ **线** —— 造成 10 点伤害，获得 2 点[gold]力道[/gold]。升级后 11 点伤害。')

card('ZongHeng', '纵横', 1, 'Skill', 'Uncommon', 'Self', aspect='Line',
     vars_=[vi('Force', 2), vi('Bonus', 2)],
     play=[pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 2)'),
           getpw('ZongHengPower', [('Amount', 'Bonus', 1)])],
     upg=['        DynamicVars["Bonus"].UpgradeValueBy(1);'],
     doc='★ **线** —— 获得 2 点[gold]力道[/gold]。本回合每次获得力道时额外 +1。\n'
         '升级后额外 +2。★ 和「勾线」这类牌形成滚雪球。')

card('XianTiao', '线条', 1, 'Attack', 'Uncommon', 'AnyEnemy', aspect='Line',
     vars_=[DV % 9, vc(1)],
     play=[TC, atk('DynamicVars.Damage.BaseValue'), draw('DynamicVars.Cards.IntValue')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(3m);'],
     doc='★ **线** —— 造成 9 点伤害，抽 1 张牌。升级后 10 点伤害。')

card('ZhiGuan', '直贯', 2, 'Attack', 'Uncommon', 'AnyEnemy', aspect='Line',
     vars_=[DV % 17],
     play=[TC, atk('DynamicVars.Damage.BaseValue + (MyLiDao >= 3 ? DynamicVars.Damage.BaseValue : 0m)')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(4m);'],
     doc='★ **线** —— 造成 17 点伤害；若你的[gold]力道[/gold]不低于 3，**伤害翻倍**。\n'
         '升级后 18 点。★ 线流的兑现点。')

card('ChanSi', '缠丝', 1, 'Skill', 'Uncommon', 'Self', aspect='Line', gains_block=True,
     vars_=[BV % 8, vi('Force', 2)],
     play=[block('DynamicVars.Block.BaseValue'),
           pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 2)')],
     upg=['        DynamicVars.Block.UpgradeValueBy(2m);'],
     doc='★ **线** —— 获得 8 点格挡，获得 2 点[gold]力道[/gold]。升级后 8 点格挡。')

card('YunJinChengFeng', '运斤成风', 2, 'Skill', 'Uncommon', 'Self', aspect='Line',
     vars_=[vi('Force', 3), vc(2)],
     play=[pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 3)'),
           draw('DynamicVars.Cards.IntValue')],
     upg=['        DynamicVars["Force"].UpgradeValueBy(1);'],
     doc='★ **线** —— 获得 2 点[gold]力道[/gold]，抽 2 张牌。升级后 4 点力道。')

card('BiFengYiZhuan', '笔锋一转', 1, 'Skill', 'Uncommon', 'Self', aspect='Line',
     vars_=[vi('Force', 1)],
     play=[pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 1)'),
           pw('LiDaoPower', 'MyLiDao')],
     upg=['        DynamicVars["Force"].UpgradeValueBy(1);'],
     doc='★ **线** —— 将你的[gold]力道[/gold]**翻倍**（先 +1 再叠加当前层数）。升级后基础 +2。\n'
         '★ 线流的爆发按钮。')

card('YiXianTian', '一线天', 0, 'Attack', 'Uncommon', 'AnyEnemy', aspect='Line',
     cond='LiDaoAtLeast(5)', vars_=[DV % 24], ex=True,
     play=[TC, atk('DynamicVars.Damage.BaseValue')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(6m);'],
     doc='★ **线 · 条件牌** —— **需[gold]力道[/gold]不低于 5** 才能打出。\n'
         '造成 24 点伤害。[消耗] 升级后 26 点。\n'
         '★ 用「勾线 / 长线」把力道垫起来，这张 0 费牌就是白送的 20 点。')

card('ChuanZhenYinXian', '穿针引线', 1, 'Attack', 'Uncommon', 'AnyEnemy', aspect='Line',
     vars_=[DV % 8, vi('PerForce', 3)],
     play=[TC, atk('DynamicVars.Damage.BaseValue + MyLiDao * DynamicVars.GetIntOrDefault("PerForce", 2)')],
     upg=['        DynamicVars["PerForce"].UpgradeValueBy(1);'],
     doc='★ **线** —— 造成 8 点伤害；你每有 1 点[gold]力道[/gold]，此伤害 +2。升级后每点 +3。\n'
         '★ 线流的兑现口：力道垫得越高，这一击越重。')

card('ChangQuZhiRu', '长驱直入', 2, 'Attack', 'Uncommon', 'AnyEnemy', aspect='Line',
     vars_=[DV % 14, vi('Force', 2)],
     play=[TC, atk('DynamicVars.Damage.BaseValue'),
           pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 2)')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(4m);'],
     doc='★ **线** —— 造成 14 点伤害，获得 2 点[gold]力道[/gold]。升级后 16 点伤害。')

card('LiDaoHuiLiu', '力道回流', 1, 'Skill', 'Uncommon', 'Self', aspect='Line',
     vars_=[vi('Bonus', 2)],
     play=['        var force = await ClearLiDao(choiceContext);\n',
           '        var n = force + DynamicVars.GetIntOrDefault("Bonus", 1);\n',
           '        if (n > 0)\n        {\n',
           '            await Draw(choiceContext, n);\n        }\n'],
     upg=['        DynamicVars["Bonus"].UpgradeValueBy(1);'],
     doc='★ **线** —— 消耗所有[gold]力道[/gold]，抽等同层数 +1 的牌。升级后 +2。\n'
         '★ 力道用不完时的回收口，配合「笔锋一转」可以爆抽。')

# ---- 面 ×11 ----
card('PoMoV5', '泼墨', 2, 'Attack', 'Uncommon', 'AllEnemies', aspect='Face',
     vars_=[DV % 14], play=[dmg_all('DynamicVars.Damage.BaseValue')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(4m);'],
     doc='★ **面** —— 对**所有**敌人造成 14 点伤害。升级后 16 点。')

card('DaPoMo', '大泼墨', 3, 'Attack', 'Uncommon', 'AllEnemies', aspect='Face',
     vars_=[DV % 24], play=[dmg_all('DynamicVars.Damage.BaseValue')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(6m);'],
     doc='★ **面** —— 对**所有**敌人造成 24 点伤害。升级后 26 点。\n'
         '★ 3 费群体 —— 靠笔锋减费打出才是它的正确用法。')

card('ManZhiYunYan', '满纸云烟', 2, 'Skill', 'Uncommon', 'Self', aspect='Face', gains_block=True,
     vars_=[BV % 17], play=[block('DynamicVars.Block.BaseValue')],
     upg=['        DynamicVars.Block.UpgradeValueBy(4m);'],
     doc='★ **面** —— 获得 17 点格挡。升级后 18 点。')

card('WanHeQianYan', '万壑千岩', 3, 'Skill', 'Uncommon', 'Self', aspect='Face', gains_block=True,
     vars_=[BV % 26], play=[block('DynamicVars.Block.BaseValue')],
     upg=['        DynamicVars.Block.UpgradeValueBy(6m);'],
     doc='★ **面** —— 获得 26 点格挡。升级后 28 点。')

card('MoYunTianChengV5', '墨韵天成', 2, 'Skill', 'Uncommon', 'Self', aspect='Face',
     vars_=[vi('MoYun', 3), vc(2)],
     play=[pw('MoYunPower', 'DynamicVars.GetIntOrDefault("MoYun", 3)'),
           draw('DynamicVars.Cards.IntValue')],
     upg=['        DynamicVars["MoYun"].UpgradeValueBy(1);'],
     doc='★ **面** —— 获得 2 点[gold]墨韵[/gold]，抽 2 张牌。升级后 4 点墨韵。\n'
         '★ 细水长流的核心铺场牌。')

card('NongMoZhongCai', '浓墨重彩', 2, 'Attack', 'Uncommon', 'AnyEnemy', aspect='Face',
     vars_=[DV % 13, vi('MoYunCost', 3), vi('Bonus', 11)],
     play=['        var cost = Math.Max(0, DynamicVars.GetIntOrDefault("MoYunCost", 3));\n',
           '        if (cost > 0 && MyMoYun >= cost)\n        {\n',
           '            await MoYunPower.Gain(choiceContext, Owner!.Creature, -cost);\n',
           '        }\n',
           TC, atk('DynamicVars.Damage.BaseValue + (MyMoYun >= cost ? DynamicVars.GetIntOrDefault("Bonus", 9) : 0m)')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(2m);'],
     doc='★ **面** —— 造成 13 点伤害；**消耗 3 点[gold]墨韵[/gold]**，此牌伤害 +9。升级后 13 点。\n'
         '★ 墨韵的兑现口 —— 囤够了就打。')

card('JuanZhou', '卷轴', 2, 'Skill', 'Uncommon', 'Self', aspect='Face',
     vars_=[vc(4)], play=[draw('DynamicVars.Cards.IntValue')],
     upg=['        DynamicVars.Cards.UpgradeValueBy(1);'],
     doc='★ **面** —— 抽 4 张牌。升级后抽 5 张。')

card('PoTianV5', '泼天', 3, 'Attack', 'Uncommon', 'AllEnemies', aspect='Face', ex=True,
     vars_=[vi('PerMoYun', 4)],
     play=['        var mo = MyMoYun;\n',
           '        if (mo > 0)\n        {\n',
           '            await MoYunPower.Gain(choiceContext, Owner!.Creature, -mo);\n        }\n',
           '        if (mo > 0)\n        {\n',
           '            await DealDamageToAll(choiceContext, mo * DynamicVars.GetIntOrDefault("PerMoYun", 3));\n        }\n'],
     upg=['        DynamicVars["PerMoYun"].UpgradeValueBy(1);'],
     doc='★ **面** —— **消耗所有[gold]墨韵[/gold]**，每点对所有敌人造成 3 点伤害。[消耗]\n'
         '升级后每点 4 点。★ 墨韵越厚越恐怖 —— 细水长流的终极兑现。')

card('LiYaQianJun', '力压千钧', 2, 'Attack', 'Uncommon', 'AnyEnemy', aspect='Face',
     vars_=[DV % 16],
     play=[TC, atk('DynamicVars.Damage.BaseValue')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(4m);'],
     doc='★ **面** —— 造成 16 点伤害。若你的[gold]墨韵[/gold]不低于 10，此牌**费用变为 0**。\n'
         '升级后 17 点。★ 高墨韵的额外奖励（由卡牌基类的减费钩子配合判定）。')

card('MoJin', '墨尽', 1, 'Attack', 'Uncommon', 'AnyEnemy', aspect='Face',
     cond='HandCountAtMost(1)', vars_=[DV % 26],
     play=[TC, atk('DynamicVars.Damage.BaseValue')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(6m);'],
     doc='★ **面 · 条件牌** —— **需手牌不多于 1 张** 才能打出。造成 26 点伤害。\n'
         '升级后 28 点。\n'
         '★ 套路①：用别的牌把手牌打空 → 这张 1 费牌就是全模组最高性价比。')

card('KuangCao', '狂草', 1, 'Skill', 'Uncommon', 'Self', aspect='Face',
     vars_=[vc(5), vi('BloodCost', 5)],
     play=['        if (Owner is { } self)\n        {\n',
           '            await CreatureCmd.Damage(choiceContext, self.Creature,\n',
           '                DynamicVars.GetIntOrDefault("BloodCost", 5), ValueProp.Move, self.Creature, null, null);\n',
           '        }\n', draw('DynamicVars.Cards.IntValue')],
     upg=['        DynamicVars.Cards.UpgradeValueBy(1);'],
     doc='★ **面 · 高过牌+代价** —— 抽 5 张牌，**受到 5 点伤害**。升级后抽 6 张。\n'
         '★ 套路③：效率越高代价越高。这张是全模组最强过牌，代价也是真的。')



# ============ 无限流四件套（回锋已在罕见段，这里补另外三件）============
card('JieBi', '接笔', 0, 'Skill', 'Rare', 'Self', aspect='Point',
     vars_=[vc(1)],
     play=['        var lit = Owner is { } p && HuiFengTracePower.WasPlayedThisTurn(p.Creature);\n',
           draw('DynamicVars.Cards.IntValue'),
           '        if (lit)\n        {\n            await GainEnergy(1);\n        }\n'],
     upg=['        DynamicVars.Cards.UpgradeValueBy(1);'],
     doc='★ **点 · 无限流二环** —— 0 费：抽 1 张牌。\n'
         '**若你本回合打出过「回锋」**，额外获得 1 点能量。\n'
         '升级后抽 1 张。\n'
         '★ 单独用就是一张「0 费抽 1」，很普通。价值在于**给回锋回血**：\n'
         '   回锋自伤 → 接笔亮灯产能量 → 能量再喂给下一个循环。')

card('XuZhi', '续纸', 0, 'Skill', 'Rare', 'Self', aspect='Line',
     vars_=[vc(1)],
     play=[draw('DynamicVars.Cards.IntValue')],
     upg=['        DynamicVars.Cards.UpgradeValueBy(1);'],
     doc='★ **线 · 无限流三环** —— 0 费：抽 1 张牌。升级后抽 1 张。\n'
         '★ 四件套里最朴素的一张，作用是**把环补上**：\n'
         '   没有它，回锋和接笔会卡在手牌不够；有了它，手牌才能持续流动。')

card('WuShiWuZhong', '无始无终', 2, 'Power', 'Rare', 'Self', aspect='Face',
     vars_=[vi('Cap', 2)],
     play=[getpw('WuShiWuZhongPower', [('Cap', 'Cap', 2)])],
     upg=['        DynamicVars["Cap"].UpgradeValueBy(1);'],
     doc='★ **面 · 无限流引擎** —— 每回合最多触发 2 次：\n'
         '你打出「点」牌时，抽 1 张牌。升级后上限 3 次。\n'
         '★ 四件套里最贵、最慢的一件，却是把环咬合的关键。\n'
         '★ 完整链条：回锋（挂牌+自伤）→ 接笔（产能量）→ 续纸（补手牌）→ 无始无终（点牌回手）。\n'
         '   代价是每循环一次掉 1 点生命 —— 无限 = 无限掉血，自带死亡倒计时。')



# ============================================================================
# 稀有 ×22（点 7 / 线 7 / 面 8 —— 另 3 张「无限流」已在上方）
# ============================================================================
# ---- 点 ×7 ----
card('RunBi', '润笔', 1, 'Power', 'Rare', 'Self',
     vars_=[vi('Cap', 2)],
     play=[getpw('RunBiPower', [('Cap', 'Cap', 2)])],
     upg=['        DynamicVars["Cap"].UpgradeValueBy(1);'],
     doc='★ **偏激流 · 纯点** 的支撑能力 ——\n'
         '每当你打出一张「点」牌，获得 1 点能量（每回合上限 2 次）。升级后上限 3 次。\n'
         '★ 让「只堆点牌」成为一条能赢的路线：点牌本身数值低，但打出它就能换能量。\n'
         '★ 三张偏激流能力**互斥**，逼你选边。')

card('DianJingZhiBi', '点睛之笔', 0, 'Skill', 'Rare', 'Self', aspect='Point',
     vars_=[vi('Edge', 3), vc(2)],
     play=[pw('BiFengPower', 'DynamicVars.GetIntOrDefault("Edge", 3)'),
           draw('DynamicVars.Cards.IntValue')],
     upg=['        DynamicVars["Edge"].UpgradeValueBy(1);'],
     doc='★ **点** —— 0 费：获得 2 点[gold]笔锋[/gold]，抽 2 张牌。升级后 4 点笔锋。\n'
         '★ 一次把费用垫足 + 补手牌，是「面」牌的完美前置。')

card('WanDianGuiYi', '万点归一', 1, 'Attack', 'Rare', 'AnyEnemy', aspect='Point',
     vars_=[DV % 4, vr(4)],
     play=[TC, atk('DynamicVars.Damage.BaseValue', hits='DynamicVars.Repeat.IntValue')],
     upg=['        DynamicVars.Repeat.UpgradeValueBy(1);'],
     doc='★ **点** —— 造成 4 点伤害，共 4 次。升级后 ★ 质变：5 次。\n'
         '★ 每一段都吃[gold]力道[/gold]加成，是「点 + 线」混搭的最佳载体。')

card('ShuMiYouZhi', '疏密有致', 1, 'Skill', 'Rare', 'Self', aspect='Point',
     vars_=[vi('Bonus', 0)],
     play=['        var edge = await ClearBiFeng(choiceContext);\n',
           '        var n = edge + DynamicVars.GetIntOrDefault("Bonus", 0);\n',
           '        if (n > 0)\n        {\n',
           '            await Draw(choiceContext, n);\n        }\n'],
     upg=['        DynamicVars["Bonus"].UpgradeValueBy(1);'],
     doc='★ **点** —— 消耗所有[gold]笔锋[/gold]，每点抽 1 张牌。升级后额外 +1 张。\n'
         '★ 笔锋用不完时的泄洪口。')

card('HaoLi', '毫厘', 0, 'Attack', 'Rare', 'AnyEnemy', aspect='Point',
     vars_=[vi('PerEdge', 3)],
     play=[TC, atk('MyBiFeng * DynamicVars.GetIntOrDefault("PerEdge", 2)')],
     upg=['        DynamicVars["PerEdge"].UpgradeValueBy(1);'],
     doc='★ **点** —— 0 费：造成等同你[gold]笔锋[/gold] 2 倍的伤害。升级后 3 倍。\n'
         '★ 笔锋既是费用又是弹药 —— 用还是留，是个真决策。')

card('DianShiChengJin', '点石成金', 2, 'Skill', 'Rare', 'Self', aspect='Point',
     vars_=[vi('Edge', 5)],
     play=[pw('BiFengPower', 'DynamicVars.GetIntOrDefault("Edge", 5)')],
     upg=['        DynamicVars["Edge"].UpgradeValueBy(1);'],
     doc='★ **点** —— 获得 5 点[gold]笔锋[/gold]。升级后 6 点。\n'
         '★ 一张牌直接把后面 3 费的面牌变成 0 费 —— 均衡流的发动机。')

card('MoShou', '墨守', 1, 'Skill', 'Rare', 'Self', aspect='Point', gains_block=True,
     vars_=[BV % 5],
     play=[block('DynamicVars.Block.BaseValue + MyBiFeng * 3m')],
     upg=['        DynamicVars.Block.UpgradeValueBy(4m);'],
     doc='★ **点** —— 获得 5 点格挡，再额外获得等于你[gold]笔锋[/gold] 3 倍的格挡。\n'
         '升级后基础 8 点。★ 把「留着不用的笔锋」变成防御。')

# ---- 线 ×7 ----
card('LiTouZhiBeiV5', '力透纸背', 1, 'Power', 'Rare', 'Self',
     vars_=[vi('Force', 2)],
     play=[getpw('LiTouPower', [('Force', 'Force', 2)])],
     upg=['        DynamicVars["Force"].UpgradeValueBy(1);'],
     doc='★ **偏激流 · 纯线** 的支撑能力 ——\n'
         '每回合开始时获得 2 点[gold]力道[/gold]。升级后 3 点。\n'
         '★ 让线流有一条稳定底盘：力道越厚，线牌越强。\n'
         '★ 三张偏激流能力**互斥**。')

card('TieHuaYinGou', '铁画银钩', 2, 'Attack', 'Rare', 'AnyEnemy', aspect='Line',
     vars_=[DV % 14, vi('Force', 2)],
     play=[TC, atk('DynamicVars.Damage.BaseValue'),
           pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 2)')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(4m);'],
     doc='★ **线** —— 造成 14 点伤害，获得 2 点[gold]力道[/gold]。升级后 16 点伤害。')

card('ZhongFeng', '中锋', 1, 'Skill', 'Rare', 'Self', aspect='Line',
     vars_=[vi('Force', 5)],
     play=[pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 5)')],
     upg=['        DynamicVars["Force"].UpgradeValueBy(1);'],
     doc='★ **线** —— 获得 5 点[gold]力道[/gold]。升级后 6 点。\n★ 线流最大的一次性增幅。')

card('TieXian', '铁线', 2, 'Attack', 'Rare', 'AnyEnemy', aspect='Line',
     vars_=[vi('PerForce', 4)],
     play=[TC, atk('MyLiDao * DynamicVars.GetIntOrDefault("PerForce", 3)')],
     upg=['        DynamicVars["PerForce"].UpgradeValueBy(1);'],
     doc='★ **线** —— 造成等同你[gold]力道[/gold] 3 倍的伤害。升级后 4 倍。\n'
         '★ 线流的兑现口：力道垫到 8 点就是 24 伤。')

card('XingYunLiuShui', '行云流水', 1, 'Skill', 'Rare', 'Self', aspect='Line',
     vars_=[vi('Force', 3), vc(1)],
     play=[pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 3)'),
           draw('DynamicVars.Cards.IntValue')],
     upg=['        DynamicVars["Force"].UpgradeValueBy(1);'],
     doc='★ **线** —— 获得 3 点[gold]力道[/gold]，抽 1 张牌。升级后 4 点力道。')

card('BiLaoMoXiu', '笔老墨秀', 2, 'Skill', 'Rare', 'Self', aspect='Line',
     vars_=[vi('Bonus', 0)],
     play=['        var force = await ClearLiDao(choiceContext);\n',
           '        var n = force + DynamicVars.GetIntOrDefault("Bonus", 0);\n',
           '        if (n > 0)\n        {\n            await GainEnergy(n);\n        }\n'],
     upg=['        DynamicVars["Bonus"].UpgradeValueBy(1);'],
     doc='★ **线** —— 消耗所有[gold]力道[/gold]，每点获得 1 点能量。升级后额外 +1 点。\n'
         '★ 力道换费用 —— 线流突然打出一波爆发的开关。')

card('QianJunYiBi', '千钧一笔', 3, 'Attack', 'Rare', 'AnyEnemy', aspect='Line', ex=True,
     vars_=[DV % 26, vi('Force', 4)],
     play=[TC, atk('DynamicVars.Damage.BaseValue'),
           pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 4)')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(6m);'],
     doc='★ **线** —— 造成 26 点伤害，获得 4 点[gold]力道[/gold]。[消耗]\n'
         '升级后 28 点。★ 3 费终结技，要留着给关键时刻。')

# ---- 面 ×8 ----
card('PoTianAbility', '泼天', 2, 'Power', 'Rare', 'Self',
     vars_=[vi('PerStep', 4)],
     play=[getpw('PoTianPower', [('PerStep', 'PerStep', 3)])],
     upg=['        DynamicVars["PerStep"].UpgradeValueBy(2);'],
     doc='★ **偏激流 · 纯面** 的支撑能力 ——\n'
         '[gold]墨韵[/gold]**不再削弱「点 / 线」牌**，改为每满 5 层，你的「面」牌伤害 +3。\n'
         '升级后 +5。\n'
         '★ 把墨韵从「双刃」变成「纯增益」——「只堆面牌」成为越打越强的路线。\n'
         '★ 代价：你彻底放弃点/线，前期会很难受。三张偏激流能力**互斥**。')

card('WanShanHongBian', '万山红遍', 3, 'Attack', 'Rare', 'AllEnemies', aspect='Face', ex=True,
     vars_=[DV % 29, vi('MoYunCost', 5), vi('Bonus', 14)],
     play=['        var cost = Math.Max(0, DynamicVars.GetIntOrDefault("MoYunCost", 5));\n',
           '        var boosted = MyMoYun >= cost;\n',
           '        if (boosted)\n        {\n',
           '            await MoYunPower.Gain(choiceContext, Owner!.Creature, -cost);\n',
           '        }\n',
           dmg_all('DynamicVars.Damage.BaseValue + (boosted ? DynamicVars.GetIntOrDefault("Bonus", 12) : 0m)')],
     upg=['        DynamicVars.Damage.UpgradeValueBy(6m);'],
     doc='★ **面** —— 对**所有**敌人造成 29 点伤害。[消耗]\n'
         '**消耗 5 点墨韵**，改为 36 点。升级后 30 / 42。\n'
         '★ 细水长流的群体兑现。')

card('MoHai', '墨海', 2, 'Skill', 'Rare', 'Self', aspect='Face',
     vars_=[vi('MoYun', 6), vc(1)],
     play=[pw('MoYunPower', 'DynamicVars.GetIntOrDefault("MoYun", 6)'),
           draw('DynamicVars.Cards.IntValue')],
     upg=['        DynamicVars["MoYun"].UpgradeValueBy(2);'],
     doc='★ **面** —— 一次获得 6 点[gold]墨韵[/gold]，抽 1 张牌。升级后 8 点。\n'
         '★ 细水长流的最快铺场牌 —— 前期摸到它就等于开了加速。')

card('ShanGaoShuiChang', '山高水长', 3, 'Skill', 'Rare', 'Self', aspect='Face', gains_block=True,
     vars_=[BV % 19], play=[block('DynamicVars.Block.BaseValue')],
     upg=['        DynamicVars.Block.UpgradeValueBy(5m);'],
     doc='★ **面** —— 获得 19 点格挡。升级后 21 点。\n'
         '★ 全模组最高单次格挡。配合笔锋减费打出才划算。')

card('ChangJuan', '长卷', 2, 'Skill', 'Rare', 'Self', aspect='Face',
     vars_=[vc(6), vi('BloodCost', 6)],
     play=['        if (Owner is { } self)\n        {\n',
           '            await CreatureCmd.Damage(choiceContext, self.Creature,\n',
           '                DynamicVars.GetIntOrDefault("BloodCost", 6), ValueProp.Move, self.Creature, null, null);\n',
           '        }\n', draw('DynamicVars.Cards.IntValue')],
     upg=['        DynamicVars.Cards.UpgradeValueBy(1);'],
     doc='★ **面 · 高过牌+代价** —— 抽 6 张牌，**受到 6 点伤害**。升级后抽 7 张。\n'
         '★ 效率越高代价越高 —— 全模组最强过牌，代价也是真的。')

card('LiTouWanXiang', '力透万象', 3, 'Attack', 'Rare', 'AnyEnemy', aspect='Face', ex=True,
     vars_=[vi('PerMoYun', 3), vi('Base', 14)],
     play=[TC, atk('MyMoYun * DynamicVars.GetIntOrDefault("PerMoYun", 2) + DynamicVars.GetIntOrDefault("Base", 14)')],
     upg=['        DynamicVars["PerMoYun"].UpgradeValueBy(1);'],
     doc='★ **面** —— 造成「[gold]墨韵[/gold]层数 ×2 + 14」点伤害。[消耗]\n'
         '升级后 ×3 + 14。★ 墨韵越厚越恐怖。')

card('KaiTianPiDi', '开天辟地', 3, 'Attack', 'Rare', 'AnyEnemy', aspect='Face', ex=True,
     vars_=[DV % 31, vi('EmptyBonus', 24)],
     play=[TC, atk('DynamicVars.Damage.BaseValue'),
           '        if (HandCountAtMost(0))\n        {\n',
           '            await DealDamageToAll(choiceContext, DynamicVars.GetIntOrDefault("EmptyBonus", 20));\n',
           '        }\n'],
     upg=['        DynamicVars.Damage.UpgradeValueBy(6m);'],
     doc='★ **面 · 条件牌** —— 造成 31 点伤害。[消耗]\n'
         '**若你的手牌为空**，再对所有敌人造成 20 点伤害。升级后 32 点。\n'
         '★ 套路①的终极形态：把手牌打光换一次全场重击。')

card('JingShuiLiuShen', '静水流深', 2, 'Skill', 'Rare', 'Self', aspect='Face',
     vars_=[vi('MoYun', 3), vc(2)],
     play=[pw('MoYunPower', 'DynamicVars.GetIntOrDefault("MoYun", 3)'),
           draw('DynamicVars.Cards.IntValue')],
     upg=['        DynamicVars["MoYun"].UpgradeValueBy(1);'],
     doc='★ **面** —— 获得 2 点[gold]墨韵[/gold]，抽 2 张牌。升级后 4 点。')

# ============================================================================
# 派生 ×5（先古 2 / 事件 3）
# ============================================================================
card('WanJieRuLinAncient', '万界如林', 2, 'Skill', 'Ancient', 'Self', aspect='Face', ex=True,
     vars_=[vi('PerMoYun', 4)],
     play=['        var mo = MyMoYun;\n',
           '        if (mo <= 0)\n        {\n            return;\n        }\n',
           '        var per = Math.Max(1, DynamicVars.GetIntOrDefault("PerMoYun", 3));\n',
           '        var n = mo / per;\n',
           '        if (n > 0)\n        {\n',
           '            await GainEnergy(n);\n',
           '            await Draw(choiceContext, n);\n        }\n'],
     upg=[],
     doc='★ **先古牌** —— 每 3 点[gold]墨韵[/gold]，获得 1 点能量并抽 1 张牌。[消耗]\n'
         '[gold]保留[/gold]。★ 不直接造成伤害 —— 它把墨韵变成你继续运转的燃料。')

card('WuMingZhiShi', '无名之始', 0, 'Skill', 'Ancient', 'Self', ex=True,
     vars_=[vi('PerTwo', 1)],
     play=['        var edge = await ClearBiFeng(choiceContext);\n',
           '        var force = await ClearLiDao(choiceContext);\n',
           '        var mo = MyMoYun;\n',
           '        if (mo > 0)\n        {\n',
           '            await MoYunPower.Gain(choiceContext, Owner!.Creature, -mo);\n        }\n',
           '        var total = edge + force + mo;\n',
           '        var gain = total / 2;\n',
           '        if (gain > 0)\n        {\n            await GainEnergy(gain);\n        }\n'],
     upg=[],
     doc='★ **先古牌** —— 将你的[gold]墨韵[/gold]、[gold]笔锋[/gold]、[gold]力道[/gold]'
         '**全部清零**，每清 2 层获得 1 点能量。[消耗]\n'
         '★ 重置按钮：墨韵堆太高拖累了点/线时，用它换一波费用重新开始。')

card('TaDeLaiChu', '她的来处', 1, 'Skill', 'Event', 'Self', aspect='Line',
     vars_=[vi('Force', 3)],
     play=[pw('LiDaoPower', 'DynamicVars.GetIntOrDefault("Force", 3)')],
     upg=[],
     doc='★ **事件牌** —— 获得 3 点[gold]力道[/gold]。\n'
         '★ 她的身世没人说得清，但她说的话总能让人多出一分力气。')

card('HuoXuShiZhenDe', '或许是真的', 2, 'Attack', 'Event', 'AnyEnemy', aspect='Face',
     vars_=[DV % 17],
     play=[TC, atk('DynamicVars.Damage.BaseValue'),
           draw('1')],
     upg=[],
     doc='★ **事件牌** —— 造成 17 点伤害，抽 1 张牌。\n'
         '★ 传闻有真有假 —— 打出它抽到的那张牌，就当是「或许是真的」。')

card('Mi', '谜', 0, 'Skill', 'Event', 'Self', aspect='Point',
     vars_=[vc(1), vi('EmptyDraw', 3)],
     play=['        var n = DynamicVars.Cards.IntValue;\n',
           '        if (HandCountAtMost(1))\n        {\n',
           '            n = DynamicVars.GetIntOrDefault("EmptyDraw", 3);\n        }\n',
           draw('n')],
     upg=[],
     doc='★ **事件牌** —— 抽 1 张牌；**若你的手牌不多于 1 张**，改为抽 3 张。\n'
         '★ 「林」的身份一直是个谜 —— 你越接近答案，得到的越多。')


# ================================ 发射器 ================================
ASP = {'Point': 'Point', 'Line': 'Line', 'Face': 'Face'}


def emit(c):
    L = [HDR, '\n/// <summary>\n']
    for ln in c['doc'].split('\n'):
        L.append('/// %s\n' % ln)
    L.append('/// </summary>\n')
    if c['starter']:
        L.append('[RegisterCharacterStarterCard(typeof(WanJieRuLinCharacter), %d)]\n' % c['starter'])
    if c['archaic']:
        L.append('[RegisterArchaicToothTranscendence(typeof(%s))]\n' % c['archaic'])
    L.append('[RegisterCard(typeof(WanJieRuLinCardPool))]\n')
    L.append('public sealed class %s : WanJieRuLinCardModel\n{\n' % c['cls'])
    L.append('    public %s() : base(%s, CardType.%s, CardRarity.%s, TargetType.%s)\n    {\n    }\n\n'
             % (c['cls'], c['cost'], c['ctype'], c['rarity'], c['target']))
    if c['aspect']:
        L.append('    /// <inheritdoc />\n    public override WanJieAspect Aspect => WanJieAspect.%s;\n\n'
                 % c['aspect'])
    if c['vars']:
        L.append('    protected override IEnumerable<DynamicVar> CanonicalVars =>\n    [\n')
        for i, v in enumerate(c['vars']):
            L.append('        %s%s\n' % (v, ',' if i < len(c['vars']) - 1 else ''))
        L.append('    ];\n\n')
    kws = ['Exhaust'] if c['ex'] else []
    if kws:
        L.append('    public override IEnumerable<CardKeyword> CanonicalKeywords => [%s];\n\n'
                 % ', '.join('CardKeyword.' + k for k in kws))
    if c['gains_block']:
        L.append('    public override bool GainsBlock => true;\n\n')
    if c['cond']:
        L.append('    protected override bool? PlayCondition => %s;\n\n' % c['cond'])
    L.append('    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)\n    {\n')
    L.extend(c['play'])
    L.append('    }\n\n')
    L.append('    protected override void OnUpgrade()\n    {\n')
    L.extend(c['upg'])
    L.append('    }\n}\n')
    return ''.join(L)


print('生成 %d 张：' % len(SPEC))
for c in SPEC:
    io.open(os.path.join(OUT, c['cls'] + '.cs'), 'w', encoding='utf-8', newline='\n').write(emit(c))
    print('  %-14s %-8s %s费 %s' % (c['cls'], c['rarity'], c['cost'], c['aspect'] or '—'))
print('done')
