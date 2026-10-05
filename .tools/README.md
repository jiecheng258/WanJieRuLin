# `.tools` — 《万界如林》开发管线

> ★ **本目录是唯一真相源。** 仓库自洽：clone → 改生成器 → 跑脚本 → 打包。

## 一、完整流程

```
改卡            .tools/gen_cards_v05.py          ← 只改这一个文件
   ↓
生成 C# 卡牌      python .tools/gen_cards_v05.py
   ↓
生成卡牌文案      python .tools/gen_card_loc_v05.py   （从生成器的 doc 自动产出 zh/en）
   ↓
生成本地化 json   python .tools/gen_loc.py
   ↓
生成美术（可选）   python .tools/gen_v05_art.py
   ↓
审计（4 项全过）   python .tools/audit.py
                 python .tools/audit_placeholders.py
                 python .tools/audit_hazards.py --self-test
                 python .tools/audit_infinite.py
   ↓
打包 + 自动发布   python .tools/package.py
                 （产物输出到桌面 dist 目录，并自动推送最新包到 GitHub 的 dist 分支）
```

## 二、文件清单

| 文件 | 作用 |
|---|---|
| `gen_cards_v05.py` | ★ **卡牌生成器** —— 90 张卡的全部定义（费用/稀有度/点线面归属/效果/升级/文案） |
| `gen_card_loc_v05.py` | 卡牌文案生成器（产出 `card_loc_v05.py`） |
| `card_loc_v05.py` | 卡牌中英文案（**自动生成，别手改**） |
| `gen_loc.py` | 本地化 json 生成器（cards/powers/relics/characters/hover_tips） |
| `gen_v05_art.py` | 程序化美术生成器 |
| `audit*.py` | 6 项自动审计 |
| `package.py` | 打包 + 自动发布 |
| `publish.py` | 把最新 install 包推到 GitHub `dist` 分支 |
| `balance_*.py` | 历史平衡脚本（留痕用，一般不需要跑） |

## 三、★★ 三条铁律

1. **不要手改 `WanJieRuLinCode/Cards/*.cs`** —— 它们由 `gen_cards_v05.py` 产出，
   下次生成会被覆盖。**要改卡就改生成器。**
2. **不要手改 `localization/*.json`** —— 由 `gen_loc.py` 覆盖。
   文案改 `gen_cards_v05.py` 里每张卡的 `doc=`，或 `gen_loc.py` 的 `POWERS` 表。
3. **数值必须对照原版基线**（见下），不要凭感觉。

## 四、卡牌的写法（生成器里的规格 DSL）

```python
card('类名', '中文名', 费用, '类型', '稀有度', '目标', aspect='点/线/面',
     vars_=[DV % 伤害, BV % 格挡, vc(抽牌), vr(段数), vi('命名变量', 值)],
     play=[...出牌逻辑...],
     upg=['...升级逻辑...'],
     doc='★ **点** —— 效果描述。升级后 …。')
```

- `aspect` 决定三类归属（`Point` / `Line` / `Face`），会发射成 C# 的 `Aspect` 重写
- `doc` 会被 `gen_card_loc_v05.py` 自动转成卡面文案（数字自动变占位符）
  ★ **格式要求**：`★ **类** —— 效果正文。升级后 …。` —— 破折号之后的才是效果正文

## 五、数值基线（原版中位数 —— 调数值的唯一尺子）

| 费用 | 点（低于） | 线（持平） | 面（高于） |
|---|---|---|---|
| 1 费攻击 | 4–6 | 7 | — |
| 2 费攻击 | — | 8–13 | 14–17 |
| 3 费攻击 | — | 19 | 22–26 |
| 1 费格挡 | 4–5 | 6 | — |
| 2 费格挡 | — | 11 | 13–16 |

## 六、三个核心机制（都在 `WanJieRuLinCode/Powers/`）

| 机制 | 类 | 效果 |
|---|---|---|
| **笔锋** | `BiFengPower` | 点牌产出。**本回合最多触发 3 次，每次获得 1 点能量**（回合末清零） |
| **力道** | `LiDaoPower` | 线牌产出。本回合你打出的牌伤害与格挡 +层数 |
| **墨韵** | `MoYunPower` | 面牌产出，跨回合累积。每满 5 层：**点牌 −1**、**面牌 +2** |

> 笔锋曾是「给下一张牌减费」，但费用修改的钩子是原版 `private protected` 方法，
> **模组无法重写**，所以改成产能量（`PlayerCmd.GainEnergy`，稳定且可验证）。

## 七、踩过的坑（省点时间）

1. **删卡会破坏存档** —— 玩家牌组引用已删卡类时会变成「弃用卡牌」占位符。删卡要提醒开新档。
2. **改数值必须同步 doc 文案数字** —— 文案生成器靠数字匹配插入占位符，不同步会失配。
3. **改了卡名/删了卡，必须重建 `WanJieRuLinCardPool.SkillCardTypes`**（手写 typeof 清单）。
4. **类名不能和命名空间同名**（`WanJieRuLin` 这个类名会与 namespace 冲突）。
5. **改完 DLL 必须完全退出游戏再重开** —— RitsuLib 只在启动时加载。
