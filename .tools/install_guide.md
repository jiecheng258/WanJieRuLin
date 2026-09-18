# 万界如林（WanJieRuLin）— Slay the Spire 2 自定义角色 Mod

版本 **v0.1.2** ｜ 游戏版本 **v0.111.0** ｜ 依赖 **RitsuLib 0.6.2**

---

## 一、安装

### 前置：RitsuLib

本 Mod 依赖 RitsuLib（0.6.2 或更高）。若尚未安装，把 `STS2-RitsuLib.dll`
系列文件放入游戏目录的 `mods/STS2-RitsuLib/`。

### 安装本 Mod

把 `WanJieRuLin` 整个文件夹解压到游戏 `mods/` 目录下，最终结构应为：

```
Slay the Spire 2/
└─ mods/
   ├─ STS2-RitsuLib/
   │  └─ STS2-RitsuLib.dll ...
   └─ WanJieRuLin/
      ├─ WanJieRuLin.dll
      ├─ WanJieRuLin.json
      └─ WanJieRuLin.pck
```

游戏安装目录（Steam 默认）：

```
C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2
```

启动游戏时会弹出「已加载 Mod」的提示，接受即可。进入「新游戏」即可在角色
选择界面看到 **万界如林**。

> ⚠️ **更新 Mod 后必须完全退出游戏进程再重开。**
> RitsuLib 只在游戏启动时加载 DLL，进程还活着就永远是旧代码在内存里。
> 只回主菜单是不够的。

---

## 二、角色概览

| 项目 | 数值 |
|---|---|
| 血量 | 70 |
| 初始金币 | 99 |
| 初始遗物 | 鬼墨 |
| 初始牌组 | 打击 ×4、防御 ×4、绘 ×2 |

### 鬼气

本角色的**第二资源**，机制**类似星辉（Stars）**——独立于能量的第二种费用：

- 一部分卡牌需要「耗费鬼气」才能打出，牌面第一行会写明数值。
- 一场战斗内**跨回合保留**，不会每回合清零。
- 进入**下一场战斗时重置为 1 点**。
- 上限 20（硬上限 99），「鬼墨」每回合开始额外提供 1 点。

---

## 三、版本记录

### v0.1.2 — 第十四轮：文案全面润色（纯文案，无数值改动）

> 以**游戏原版中文**为标尺校订卡面、能力与悬浮提示。
> 原版字符串直接从 `SlayTheSpire2.pck` 里按字节提取（该文件本地化数据为明文存储）。

#### 1. 中文排版：数字与量词之间不加空格

统计原版中文字符串：

| 写法 | 原版出现次数 |
|---|---|
| `{Var}点`（无空格） | **422** |
| `{Var} 点`（有空格） | 1 |
| `{Var}层`（无空格） | **72** |
| `{Var} 层`（有空格） | 0 |

本模组原先 **188 处全部带空格**，已全部改为对齐原版。
提取 pck 的方法：不做完整 PCK 解析，直接在原始字节里 `find` 关键字
（本地化 JSON 在 pck 内是明文），比逆向 v3 头部结构可靠得多。

#### 2. 「等量鬼气」补上换算依据

原文案「获得等量鬼气」没说等量于什么，玩家无法核验。改为显式写法：

- 傲慢（卡 + 能力）：`你每造成 1 点伤害，就获得 1 点鬼气`
- 鬼气森森 / 厉鬼复苏 / 能量↔鬼气（能力）/ 光明预言：`等量` → `同等数值的`

#### 3. X 费卡的 X 补上来源

| 卡牌 | 补充 | X 的真实来源 |
|---|---|---|
| 墨染江山（卡 + 先古选项） | （X 为你耗费的鬼气） | `GhostQiXValue(cardPlay)` |
| 我不玩了 | （X 为你耗费的鬼气） | `GhostQiXValue(cardPlay)` |
| 平行世界 | （X 为你以此法弃掉的牌数） | **玩家自选弃牌数**，与鬼气无关 |

> ★ **平行世界踩坑记录**：一度按「X 费卡」惯性标注成「X 为你耗费的鬼气」，
> 被 `audit_ghostqi.py` 拦下 —— 该卡 `SetGhostQiCost` 并未调用，代码不收费，
> 卡面却出现了费用行。教训：**任何 X 都要回源码确认语义**，不能凭卡牌分类推断。

#### 4. 力量削减对齐原版句式

原版三种写法中选最贴近的一种：`敌人失去N点力量`（原版 MALAISE 同款）。
光明切割、盲心剑从「使目标失去 N 点力量」改为「敌人失去 N 点力量」。
（其余两种原版写法为 `使一名敌人在本回合失去…` / `所有敌人在本回合失去…`。）

#### 5. 鬼气说明去重

原先悬浮提示与 `cards.json` 各有一份措辞不同的鬼气说明，已抽成共用常量
`_GHOST_QI_ZH_DESC`，两处引用同一份。

#### 本轮改动清单

8 张卡牌、3 个能力、1 张先古选项、鬼气悬浮提示、角色描述。
改动全部落在唯一真相源 `.tools/gen_loc.py`，**未改任何数值或机制**。
四项审计（hazards / placeholders / ghostqi / 综合）全部通过。

---

### v0.1.1 — 第十二轮：先古选项「墨染江山」彻底修复 + 只加强不削弱

> 本轮把修复并入 `master` 主线，并**删除**了 `balance-vanilla-parity` 平衡分支。
> 合并原则：**只带入「加强」性质的改动，丢弃「削弱」性质的改动。**

**修复（两层问题，缺一不可）**

| 层 | 症状 | 根因 | 修复 |
|---|---|---|---|
| 第一层 | `InvalidOperationException: must be added to a RunState before adding it to your deck.` | 手工克隆的牌**未登记进 `RunState`**；`CardPileCmd.Add` 对 `PileType.Deck` 有 `ContainsCard`（**引用相等**）守卫 | 改用官方入口 `runState.CreateCard(canonical, player)` 一次完成「可变 + 设 Owner + 登记 + `AfterCreated`」 |
| 第二层 | 牌进牌组了，但**界面不结束、不回地图** | 选项回调**没有推进事件** → `_currentOptions` 永不为空 → `IsFinished` 永远 false → 不生成 `PROCEED` 按钮 → 软锁 | 补调 `ancient.StartPreFinished()` |

**★ 关于推进入口的 API 可见性**（实测结论，写代码前必看）：

| 方法 | 可见性 | 能否从 Mod 调用 |
|---|---|---|
| `EventModel.SetEventFinished(LocString)` | `Family`（protected） | ❌ |
| `EventModel.SetEventState(LocString, IEnumerable<EventOption>)` | `Family`（protected） | ❌ |
| `AncientEventModel.Done()` | `Family`（protected） | ❌ |
| `EventModel.EnsureCleanup()` | `Public`，但只调空的 `OnEventFinished()`，**不设 `IsFinished`** | ⚠️ 无效 |
| **`AncientEventModel.StartPreFinished()`** | **`Public`** | ✅ **唯一合法入口** |

`StartPreFinished()` 内部会读 `CustomDonePage` 或原版资源 `<先古Id>.pages.DONE.description`
（Neow / Darv 都有），再调 `SetEventFinished` 走完整推进链路。**不需要自己写结束文案。**

**护栏**：整个加牌逻辑用 `try/finally` 包住，`finally` 里无条件推进事件。
即使玩家 / `RunState` 转型失败或加牌抛异常，也只会「少给一张牌」，**不会再次软锁**。

**同类造牌写法一并修正**：

- `Cards/YinSen.cs` —— 战斗内造衍生物改走 `combatState.CreateCard(canonical, player)`
- `Relics/BeiLeiMao.cs` —— `canonical.CreateCloneForPlayer(player)` 改为
  `canonical.ToMutable()` + 设 `Owner`

> 说明：遗物造牌可以只 `ToMutable` + 设 `Owner` 就交给框架（遗物没有入堆守卫），
> **但卡牌不行**。这个区别曾经坑过一轮。

**平衡（只加强）**

| 卡牌 | 改动 |
|---|---|
| 持续侵扰 | 伤害 10 → **14**（升级 14 → **18**） |
| 声闻自 | 伤害 6 → **10** |

**已丢弃的削弱项**（未带入 `master`）：鬼域（4 效果 → 2）、阴森森（9×3 → 5×2）、
DaJi 纯注释改动，以及对应的本地化行。

**文案纠正**

- **打击**：卡面原先写着「获得 1 点鬼气」，但 `OnPlay` 里**从来没有**这段逻辑
  （只调 `DamageCmd.Attack`），是纯文案残留。已把描述与代码同步，
  改为只显示「造成 {Damage} 点伤害」。原版打击 = 1 费 6 伤无附加，
  同步描述到代码是最保守的做法。

---

### v0.1.0 — 第九轮：先古选项**依然**点了没反应 → 造牌必须走官方入口（分支 `balance-vanilla-parity`）

**现象**：第八轮修掉 `CanonicalModelException` 后，在先古之民界面选「墨染江山」**仍然无法继续下一步**。

**日志根因**（`godot.log`，出现 1 次）—— 只是把问题推进到了下一层：
```
System.NullReferenceException
  at CardModel.CreateClone()
  at CardModel.CreateCloneForPlayer(Player player)
  at WanJieRuLin.Ancients.WanJieRuLinAncientOptions.GrantMoRanJiangShan
  at MegaCrit.Sts2.Core.Events.EventOption.Chosen()
```

**根因**：`ToMutable()` 只解决「可变性」，**不解决「这张牌还没有堆（Pile）」**。
反编译 IL 拿到确凿证据：

```
CardModel.CreateClone() 的 IL（节选）
  IL_0001: call     CardModel.get_Pile
  IL_0006: brfalse.s                    ← Pile == null 直接跳进异常分支
  IL_000E: callvirt CardPile.get_Type   ← ★ NRE 精确落点
  IL_0026: call     AbstractModel.AssertMutable()

CardModel.CreateCloneForPlayer(Player player) 的 IL 只有两句
  call    CreateClone()
  stfld   CardModel._owner = player     ← 只设 owner，从不入堆

CardModel.get_Pile()
  _owner == null ? null : Owner.Piles.FirstOrDefault(p => p.Contains(this))
```

→ 一张「还没有堆」的新牌，`get_Pile()` 必然 null，`CreateCloneForPlayer` 必然 NRE，
**加不加 `ToMutable()` 都一样**。全程序集扫描确认：原版仅 **6 处**使用
`CreateClone` / `CreateDupe`，**全部处于「牌已在堆里」的上下文**，没有任何一处用它凭空造牌。
（`CreateDupe(player)` 内部转调 `CreateCloneForPlayer`，同样 NRE。）

**正解唯一形态：`ToMutable()` → 设 Owner → 交给框架**。
对照原版 `EventOption.WithRelic` 的 IL —— `ToMutable()` → `set_Owner(player)` → 交给框架，
事件给牌/给遗物的官方写法与我们逐句一致。

| 文件 | 位置 | 修改 |
|---|---|---|
| `Ancients/WanJieRuLinAncientOptions.cs` | `GrantMoRanJiangShan` | `ToMutable()` + `card.Owner = player`，再 `CardPileCmd.Add(card, PileType.Deck, CardPilePosition.Bottom, null, false)` |
| `Relics/BeiLeiMao.cs` | `BeforeCombatStart` | `ToMutable()` + `created.Owner = player`，再 `AddGeneratedCardToCombat(created, Hand, player, Random)` |
| `Cards/YinSen.cs` | `OnPlay`（洗入「阴森森」） | 抽出 `MakeToken(player)`：战斗内走 `combatState.CreateCard(canonical, player)`；否则 `ToMutable()` + `Owner = player` |

官方造牌入口的三条硬约束（均由 IL 解出）：

- `CombatState.CreateCard(canonical, owner)` = `ToMutable()` → `AddCard(mutable, owner)` → `AfterCreated()` —— 战斗内唯一正解。
- `CardPileCmd.AddGeneratedCardToCombat` 内部**不做任何克隆**，且会拒绝已有堆的牌
  （`"You are not allowed to generate cards that already have a pile"`）、
  拒绝非战斗堆（`"You are not allowed to added generated cards to a non combat pile"`）
  —— 所以**预先克隆会造成双重克隆并触发守卫异常**。它要的正是「已 ToMutable、已设 Owner、还没进堆」的牌。
- `CardPileCmd.Add(CardModel, ...)` **不设 Owner，但要求 Owner 已设**
  （否则抛 `"Attempted to add card ... but it has no owner!"`）；入堆由它负责。

**审计规则修正（规则本身曾是错误知识载体）**：旧的第 5 条规则
「未 `ToMutable()` 就克隆」把 `.ToMutable().CreateCloneForPlayer(...)` 当成**正确写法放行** ——
**这正是第八轮漏报的原因**。现改为：
**「凭空造牌时手工调用 `CreateClone` / `CreateCloneForPlayer` / `CreateDupe`」→ 无条件命中**，
并在报错信息里给出完整正解指引。自检扩到 **16 坏例 + 9 好例**，异常项 0。

**产物验证**（反编译**已部署的 DLL**，`CreateCloneForPlayer` 全程序集归零）：
```
GrantMoRanJiangShan : IL_003F CardModel.ToMutable → IL_0055 CardModel.set_Owner
YinSen.MakeToken    : IL_0026 ICombatState.CreateCard → IL_0030 ToMutable → IL_0038 set_Owner
BeiLeiMao           : IL_009A ToMutable → IL_00B0 set_Owner → IL_00E6 AddGeneratedCardToCombat
```

### v0.1.0 — 第八轮（已被第九轮取代）：先古选项第一次报错

**现象**：在先古之民（Neow / Darv）界面选「墨染江山」后，事件卡住、无法继续下一步。

**日志根因**（`godot.log`，出现 1 次）：
```
CanonicalModelException: Canonical model of type
  WanJieRuLin.Cards.MoRanJiangShan used in incorrect place.
  at AbstractModel.AssertMutable()
  at CardModel.CreateClone()
  at CardModel.CreateCloneForPlayer(Player)
  at WanJieRuLin.Ancients.WanJieRuLinAncientOptions.GrantMoRanJiangShan
  at MegaCrit.Sts2.Core.Events.EventOption.Chosen()
```
异常在 `EventOption.Chosen()` 的执行链里抛出，**把「点击选项之后的收尾流程」整段中断**，
所以事件停在原地、进退不能。

**原理**：`ModelDb.Card<T>()` 返回的是**规范（canonical / 不可变）**实例。
`CardModel.CreateClone()` 内部会 `AbstractModel.AssertMutable()` —— 规范实例必须先转成可变副本。
正确 API 是 **`CardModel.ToMutable()`**（IL: `AssertCanonical()` + `MutableClone()`）。

**修复**：3 处同类写法统一加 `.ToMutable()`。
⚠️ **注意**：本轮采用的 `.ToMutable().CreateCloneForPlayer(player)` **仍不正确**
（见第九轮 —— 加不加 `ToMutable()` 都会 NRE，因为它没解决「牌没有堆」）。
这里保留记录，只作为「问题分两层、修一层崩一层」的例证。

### v0.1.0 — 第七轮：继续压强度至原版对标（分支 `balance-vanilla-parity`）

延续第六轮的「以原版 596 张卡为标尺」，本轮按「**尽量保留机制、只调数字与文案**」
的原则处理了 5 张卡。完整对照见 `平衡调整说明.md`。

> 本轮在 git 分支 `balance-vanilla-parity` 上进行，`master` 保留第六轮结束时的状态。

| 卡牌 | 改动前 | 改动后 | 依据 |
|---|---|---|---|
| **打击** | 1 费 6 伤，卡面却写着「获得 1 点鬼气」 | 卡面改为只有「造成 {Damage} 点伤害」 | 代码里**从未**有加鬼气逻辑 —— 这是纯文案残留。**同步描述到代码**（而非给代码补效果）是最保守的做法 |
| **阴森森**（衍生物） | 0 费 **9 伤 ×3 = 27 伤** | 0 费 **5 伤 ×2 = 10 伤** | 原版 0 费衍生物 Token「Shiv」为 4 伤；27 伤是 3 费的输出却收 0 费，且「阴森」会反复洗入。保留「多段」机制 |
| **圣文字** | 1 能 + 1 鬼气 → **6 伤** + 2 易伤 + 2 虚弱 | 1 能 + 1 鬼气 → **10 伤**（升级 14）+ 同上 | 总成本 ≈ 2 能量；原版 2 费攻击 15–18 伤，扣掉易伤/虚弱附加后取 10 |
| **持续侵扰** | 2 能 + 1 鬼气 → **10 伤** + 1 易伤 + 2 虚弱 | 2 能 + 1 鬼气 → **14 伤**（升级 18）+ 同上 | 总成本 ≈ 3 能量；原版 3 费攻击 30–32 伤，扣掉附加后取 14 |
| **鬼域**（能力） | 9 鬼气，每回合 **4 个**效果 | 9 鬼气，每回合 **2 个**效果 | 原版最接近的「回响形态」3 费稀有只有约 1 个效果。删掉「第 1 张技能牌免费并抽 2 张」与「第 4 张牌获得重放」，保留「第 1 张攻击牌伤害翻倍」与「第 3 张牌费用变 0」 |

**为什么不削鬼域的费用、而是削效果**：9 鬼气已是很高的门槛（需攒 5–8 回合）。
费用不动、只降低「每回合自动产出的价值」，才能既保留「攒一管鬼气放个大招」的核心体验，
又不让它变成「放下去就赢」。

**方法备注（本轮踩到的坑）**：验证「某个效果是否真的从产物里删干净」时，
别手写 IL opcode 长度表 —— 直接从 `System.Reflection.Emit.OpCodes` 反射出每个
opcode 的 `Size` 建表，再扫 `call`/`callvirt`/`newobj` 的内联 token 解析成员名。
我手写的那版 `OpLen` 恒返回 0，等于扫描完全失效（探针会给出假的「已删干净」）。

### v0.1.0 — 第六轮：数值平衡（以原版 596 张卡为标尺）

对照从原版 `sts2.dll` 反射导出的全部 596 张卡牌数值基线，修复了 **11 项**问题。
详细对照表见同目录的 `平衡调整说明.md`。

**A. 机制 Bug：写「本回合」的能力实际是永久的（4 个）**

塔2 的能力**没有**「存活 N 回合」的堆叠类型（`PowerStackType` 只有 None/Counter/Single），
框架不会因为文案写了「本回合」就自动撤掉它 —— 必须自己实现
`AfterSideTurnEnd` + `PowerCmd.Remove(this)`。

| 能力 | 文案写的是 | 原来的真实效果 |
|---|---|---|
| 神圣暴走 | 本回合攻击伤害翻倍 | **整场**翻倍 |
| 尸瞳 | 本回合每打一张牌 +1 鬼气 | 永久 → 第 3 张牌后就是**无限鬼气** |
| 傲慢 | 本回合伤害等量转鬼气 | 永久 1:1 转化（鬼气≈能量，等于伤害直接变能量） |
| 暂避锋芒 | 本回合无法打出攻击牌 | **永久**打不出攻击牌 |

新增 `Powers/WanJieTurnScopedPower.cs` 作为「本回合限定」能力的基类，
在**自己这一方**回合结束时自我移除（`AfterSideTurnEnd` 双方回合结束都会触发，
所以要用 `participants` 判断是不是自己这一方，否则敌方回合结束就把自己撤了）。

**B. 数值写错（2 个）**

- 傲慢：伤害变量写成 **0**（升级后 1），卡面真的显示「造成 0 点伤害」→ 改为 8，升级 12
- 线，面：卡面用固定值 1 的变量当伤害公式首项，显示成「造成 1+0 点伤害」，
  实际结算「耗费鬼气+0」→ 文案改为字面 `X`，删掉该占位变量

**C. 对齐原版曲线（5 个）**

| 卡牌 | 原值 | 新值 | 依据 |
|---|---|---|---|
| 绘画分割 | 3 费 35 伤 + 返 4 鬼气 | **28 伤**（升级 36）+ 返 **2** 鬼气 | 原版 3 费顶格是重锤 32；4 鬼气 ≈ 4 费 |
| 研磨下笔 | 1 费 16 伤 | **12**（升级 16） | 原版 1 费区间 6–10，2 费才 15–18 |
| 盲心剑 | 1 费 10 伤 + 减 10 力量 | **8 伤 + 减 8 力量** | 原版刺耳尖啸 1 费全敌减 6 力量且无伤害 |
| 悠悠电龙 | 3 鬼气、罕见、重放全部牌 | **7 鬼气 + 稀有** | 原版回响形态 3 费稀有只有「第一张多打一次」 |
| 点，线 | 每鬼气 +1 伤 | 每鬼气 **+3** 伤 | 按模组自身汇率（1 鬼气≈1 能量）原值严重偏弱 |

**汇率依据**：模组自己的「审时度势」（普通）是「每 1 鬼气 → 1 能量 + 抽 1 张」，
本轮全部鬼气换算都以这个作者自己定下的 1:1 为准。

> 另有 7 项属于设计取向（打击送鬼气、绘的线性成长、鬼域的 4 效果引擎、
> 层层侵蚀的归零返还等），已在 `平衡调整说明.md` 中列出但**未改动**，等你决定。

### v0.1.0 — 第五轮：X 能量费用 + 三处变量名/文案不匹配

本轮由实机日志（3765 行）驱动，日志里共 7 类错误签名，全部修复，另主动发现并修复 1 处同类隐患。

| # | 日志里的错误签名 | 次数 | 根因 | 修复 |
|---|---|---|---|---|
| 1 | `InvalidOperationException: This card does not have an X-cost.`（打出**笔墨倾泻**时） | 6 | 这张牌要「耗费全部能量」，但 X 费用**没有真正声明**：构造函数传了 `-1`。原版根本没有「负数= X」这个约定 | 改为传 `0` + 重写 `HasEnergyCostX => true`（见下表说明） |
| 2 | `No source extension could handle the selector named "Threshold"` | 40 | 能力**鬼影森森**的悬浮提示文案里写了 `{Threshold}`，但能力本身没声明这个 DynamicVar（只在 C# 里有个同名属性） | 给 `GuiYingSenSenPower` 加 `CanonicalVars => [ModCardVars.Int("Threshold", 20)]` |
| 3 | `No source extension could handle the selector named "Bonus"`（先古事件界面） | 2 | 先古选项**直接复用了卡牌描述**。事件界面渲染选项文案时只注入事件变量，**拿不到卡牌的 DynamicVars**，描述里的 `{Bonus:diff()}` 必然解析失败 | 选项改用独立文案键 `WAN_JIE_RU_LIN_CARD_MO_RAN_JIANG_SHAN_OPTION.description`，文案里不含任何占位符 |
| 4 | `No source extension could handle the selector named "HeiAnBiZhangTempDexterity"` | 1 | 卡面占位符名少了 `Power` 后缀 | 统一改为 `HeiAnBiZhangTempDexterityPower`（见下表） |
| 5 | `KeyNotFoundException: The given key 'HeiAnBiZhangTempDexterity'` | 1 | 同 #4，代码里读变量用的名字也少了 `Power` | 同 #4 |
| 6 | `finished execution, but was in state Canceled!`（**防御**） | 1 | **连带现象**，不是「防御」本身的 bug。#1 的异常在 `PlayCardAction` 内部抛出，把出牌队列留在了取消/暂停状态，后续某次出牌就报了这句 | 无需改代码：#1 修好后自然消失。「防御」用的 `GainBlock(Creature, BlockVar, CardPlay)` 正是原版主流写法（原版卡牌用了 74 次，另一重载只用 9 次） |
| 7 | `AttacksPerGhostQi`（日志未出现，**主动排查发现**） | — | 能力**神圣暴走**的文案引用了 `{AttacksPerGhostQi}`，但能力没声明该变量（与 #2 完全同类）。只是玩家还没悬浮过它，所以日志里没体现 | 同样补 `CanonicalVars` 声明 |

#### 关于「X 能量费用」的正确写法（本条最值得记）

原版**没有任何**「费用传负数表示 X」的约定。真实机制是：

```csharp
// CardModel 里 HasEnergyCostX 默认返回 false，且是 protected virtual：
protected virtual bool HasEnergyCostX => false;

// 能量费用对象是懒构造的，X 与否就来自上面那个属性：
EnergyCost = new CardEnergyCost(this, CanonicalEnergyCost, HasEnergyCostX);

// ResolveEnergyXValue() 在非 X 费牌上会**直接抛异常**（不是返回 0）：
if (!EnergyCost.CostsX) throw new InvalidOperationException("This card does not have an X-cost.");
```

所以 X 费卡必须两条都做到（原版 Whirlwind / Skewer / MultiCast / Tempest / Malaise 都是这么写的）：

```csharp
public BiMoQingXie() : base(0, CardType.Attack, ...) { }   // 传 0，不是 -1
protected override bool HasEnergyCostX => true;            // 声明为 X 费
```

顺带把基类的读取助手做成安全的：`EnergyXValue` 现在先判 `HasEnergyCostX`，非 X 费牌返回 `0` 而不再抛异常——避免同类写法再次打断出牌流程。

#### 关于 `ModCardVars.Power<T>` 的变量命名（之前的笔记记反了）

```
ModCardVars.Power<TPower>(n)  →  变量名 == typeof(TPower).Name  （**保留结尾的 "Power"**）
```

反编译 `ModCardVars.Power` 可见它就是 `Type.GetTypeFromHandle(...).Name`，不做任何裁剪。
所以 `Power<HeiAnBiZhangTempDexterityPower>(3)` 生成的变量名是 `HeiAnBiZhangTempDexterityPower`。

审计脚本此前把「带 Power」和「去掉 Power」两种名字都塞进了白名单，导致真出问题时一声不吭。
现已修正为只认真实规则，并扩展到检查**能力文案**与**先古选项文案**（后者不允许出现任何占位符）。

### v0.1.0 — 第四轮：修复打出「绘」时游戏硬死机

**症状**：打出「绘」后游戏完全死机（无报错、无异常、日志戛然而止）——典型的**无限递归导致栈溢出**。

**根因**：本模组的临时力量/敏捷能力（`WanJieTempStrengthPower` / `WanJieTempDexterityPower`）此前基于 `ModTemporaryPowerTemplate` 自行实现，把 `OriginModel`（能力的**来源**，用于悬浮提示回溯显示）实现成了「返回自己」。渲染该能力的悬浮提示时要先取来源 → 来源又是它自己 → **无限递归**，进程直接死掉，来不及写任何日志。

「绘」是本次测试中第一张施加能力的牌，所以死机在它身上首先暴露。

**修复**：参照官方教程
<https://tutorials.sts2modding.com/docs/04-ritsulib/04-05-add-power>
的「临时能力 + 来源包装」标准写法重写：

- 新增抽象基类 `WanJieTempAppliedPower<TCard>`（`ModTemporaryAppliedPowerTemplate<TCard, WanJieTempStrengthPower>`），`OriginModel` 正确绑定到**施加它的那张卡**
- 按来源拆成 7 个具体类：`HuiTempStrength`、`HuiTempDexterity`、`MoRanJiangShanTempStrength`、`MoRanJiangShanTempDexterity`、`HeiAnBiZhangTempDexterity`、`DianLongFormTempStrength`、`HuoJianTempStrength`
- 5 处调用点（绘 / 黯然江山 / 黑暗壁障 / 电龙形态 / 火剑）全部改用新的具体类
- 本地化改为共享键 `WAN_JIE_RU_LIN_POWER_TEMP_STRENGTH` / `TEMP_DEXTERITY`（卡面变量名不变，`黑暗壁障` 的 `{WanJieTempDexterity}` 占位符不受影响）

**教训**：临时能力的 `OriginModel` 必须是"谁给的"，绝不能指向能力自身——否则悬浮提示渲染即无限递归，且表现为无日志硬死机。

### v0.1.0 — 第三轮：对照官方「次要资源」文档校正

参照官方文档
<https://tutorials.sts2modding.com/docs/04-ritsulib/04-22-7-secondary-resources>
逐项核对了鬼气的实现。结论：**主体机制本来就是基于该文档的次级资源系统**
（日志可证：`[SecondaryResource] Registered WAN_JIE_RU_LIN_SECONDARY_RESOURCE_GHOST_QI`），
但有 4 处偏离，已全部校正：

| 项目 | 之前 | 现在（按文档） |
|---|---|---|
| 卡面费用显示 | 自己拼纯文字 | 用文档推荐的 `SecondaryResourceVars.For` + `{GhostQiCost:secondaryResourceIcons()}`，渲染成**鬼气图标 + 数字**，且数值是真正的 DynamicVar（升级能用 `:diff()`） |
| X 费用 | 改用了 `SetAllowingShortfall`（非文档写法） | 回到文档标准 `SecondaryCosts().Set(id, SecondaryResourceCost.X(m))` |
| 悬浮提示本地化 | 键放在 `cards.json`，合并不到 | 新增 `localization/{zhs,eng}/static_hover_tips.json`（文档：悬浮提示默认读该表）；同时补齐 `{resourceId}.title/.description` 自动推导键 |
| 触发时机 | 金丝鬼墨在 `BeforeCombatStart` 发放开局爆发 | 移到**本场第一个回合开始**。因为资源是 `PersistencePolicy.Combat`，框架可能在该钩子之后才把数值重置到 `defaultAmount`，先前会覆盖掉 +2 |

文档核对通过的项：

- ✅ 资源注册用 `RitsuLibFramework.GetSecondaryResourceRegistry(ModId)` + `Register(localId, definition)`，ID 形如 `{MODID}_SECONDARY_RESOURCE_{LOCALID}`。
- ✅ `turnStartPolicy: None`（回合开始不自动回复）；`persistencePolicy: Combat`（战斗内保留、换战斗归位）。
- ✅ 费用用 `this.SecondaryCosts().Set(id, n)` 在构造函数里挂载。
- ✅ 战斗计数器 `NSecondaryResourceCounter` + 卡面费用 `NSecondaryResourceCardCostUi` 均已注册（日志：两条 `[NodeAttachment] Registered ...`）。
- ✅ X 数值在 `OnPlay` 用 `cardPlay.SecondaryResources().Value(id)` 读取。

### v0.1.0 — 第二轮修复

根据实机测试日志（`godot.log`）定位并修复了 6 个问题，其中 3 个会导致**战斗卡死**：

| # | 症状 | 根因 | 修复 |
|---|---|---|---|
| 1 | **打出「绘」「飘渺剑诀」等剑相关牌后卡死** | `Swords` 通过 `ModelDb.Get()` 取到的是**规范实例（不可变）**，`PowerCmd.Apply` 内部 `AssertMutable()` 直接抛 `CanonicalModelException`，把回合循环打死 | 改用泛型重载 `PowerCmd.Apply<T>(...)`，由框架派生可变实例 |
| 2 | **每次出牌都可能卡死**（日志里 52 次调用） | `YinYangGeHunXiaoPower` 与 `平行世界` 使用 `LocString.KeyPathToLocString(...)` 构造提示文案，运行期本地化表未就绪 → `IndexOutOfRangeException` | 改用原版自带的 `CardSelectorPrefs.ExhaustSelectionPrompt` / `DiscardSelectionPrompt` |
| 3 | 打出「阴森」报错 | `new YinSenSen()` 直接构造模型 → `DuplicateModelException` | 改为 `ModelDb.Card<YinSenSen>().CreateCloneForPlayer(player)` |
| 4 | 卡牌奖励界面点「黑暗壁障」报错 | `OnUpgrade()` 里变量名写成 `TemporaryDexterityPower`，而实际变量名是 `WanJieTempDexterity` → `KeyNotFoundException` | 改为正确变量名 |
| 5 | 鬼气提示框显示为原始键名 | 提示走 base 表 `static_hover_tips` 查找，我们此前把键放在了 `cards.json`，合并不到 | 新增 `localization/{zhs,eng}/static_hover_tips.json` |
| 6 | **卡面「消耗」出现两次**，分不清哪个是费用 | 费用行原本用「消耗」，与 **Exhaust 关键字**的官方译名「消耗」撞词 | 费用行统一改用「**耗费**」；22 张收费卡牌面均为「耗费 … 鬼气」 |

### v0.1.0 — 第一轮修复

| # | 问题 | 修复方式 |
|---|---|---|
| 1 | 卡面出现「升级后…」等冗余说明 | 全部移除，升级数值由游戏自动渲染 |
| 2 | 鬼气在下一场战斗叠加 | 改为星辉式语义：战斗开始显式重置为 1 点 |
| 3 | 鬼气为 0 才可打出的牌能被强打 | 可打出条件接进框架的 `IsPlayable` 补丁 |
| 4 | 绘只有一张、耗尽鬼气后无效果 | 起始 2 张；X 费用改为「有多少花多少」 |
| 5 | 大量卡牌文字显示异常 | 重写全部文案，占位符逐一核对 |
| 6 | 飘渺剑诀没有实际效果 | 剑效果改用规范实例（后又在第二轮修正为泛型 Application） |
| 7 | 鬼域打出来没有效果 | 改为出牌**前**判定的免费检测器 |
| 8 | 打出电龙形态后无法出牌 | 临时力量/敏捷改用本模组的包装能力 |
| 9 | 部分牌没真扣鬼气 | 费用行从代码解析后自动注入卡面，永不漂移 |

---

## 四、开发者：从源码构建

```
# 需要 .NET 9 SDK + Godot 4.5.1 Mono
cd "WanJieRuLin"
dotnet build WanJieRuLin.sln -c Debug
```

构建会自动编译 DLL、用 Godot headless 导出 PCK，并把两者 + 清单
复制到游戏 `mods/WanJieRuLin/`。

### 目录结构

```
WanJieRuLin/                        # Godot 项目
├─ WanJieRuLinCode/                 # C# 源码
│  ├─ Cards/                        # 52 张卡牌 + 基类 + 剑工具
│  ├─ Powers/                       # 25 个能力
│  ├─ Relics/                       # 5 个遗物 + 基类
│  ├─ Characters/                   # 角色 / 卡池 / 遗物池 / 药水池
│  ├─ Ancients/                     # 先古之民选项（复用原版 Neow / Darv）
│  ├─ ModResources.cs               # 鬼气资源定义
│  ├─ GhostQi.cs                    # 鬼气读写封装
│  └─ Entry.cs                      # 入口
├─ localization/{zhs,eng}/          # 各 6 个 JSON（含 static_hover_tips）
├─ images/                          # 卡图 / 能力图 / 遗物图 / 角色图
└─ scenes/                          # 角色场景
```

### 修改卡牌文案

文案由 `gen_loc.py` 生成（含鬼气费用自动注入：从卡牌源码解析真实费用，
所以不会和代码脱节）。改完重新运行即可覆盖 `localization/` 下的 JSON。

**注意**：卡面上的 `{VarName:diff()}` 占位符必须与代码 `CanonicalVars`
里的变量名一致，否则会显示成裸占位符。

### 写新卡时的三个坑（会导致卡死 / 崩溃）

1. **不要 `new XxxPower()` / `new XxxCard()`**
   → `DuplicateModelException`。用 `ModelDb.Power<T>()` /
   `ModelDb.Card<T>()` 再 `.ToMutable(n)`，或走上一节的官方造牌入口。

2. **不要用 `PowerCmd.Apply(ctx, 实例, ...)` 非泛型重载**
   → 传规范实例会 `CanonicalModelException` 卡死战斗。
   一律用泛型重载 `PowerCmd.Apply<T>(...)`。

3. **不要用 `LocString.KeyPathToLocString(...)`**
   → 运行期会 `IndexOutOfRangeException`。用原版自带的
   `CardSelectorPrefs.XxxSelectionPrompt`，或 `new LocString(table, key)`。

### 第四个坑（最隐蔽）：凭空造牌**绝不要手工克隆**

这条**不会崩游戏、不会弹窗**，只会让「点下去没反应」，最容易漏。
而且它有**两层**，修一层还会崩一层（第八、九轮连踩）：

```csharp
// ✗ 第 1 层报错：CanonicalModelException（规范实例不可变）
var card = ModelDb.Card<MoRanJiangShan>().CreateCloneForPlayer(player);

// ✗ 第 2 层报错：NullReferenceException（牌还没有堆，get_Pile() 返回 null）
var card = ModelDb.Card<MoRanJiangShan>().ToMutable().CreateCloneForPlayer(player);

// ✓ 正解：ToMutable() → 设 Owner → 交给框架（与官方 EventOption.WithRelic 的 IL 逐句一致）
var card = ModelDb.Card<MoRanJiangShan>().ToMutable();
card.Owner = player;
await CardPileCmd.Add(card, PileType.Deck, CardPilePosition.Bottom, null, false);
```

`CreateClone()` 的 IL 第 2 步就是 `call get_Pile; brfalse.s` —— 牌不在任何堆里就直接进异常分支；
而 `CreateCloneForPlayer` 的 IL 只有 `CreateClone() + stfld _owner`，**从不入堆**。
原版仅 6 处用 `CreateClone` / `CreateDupe`，**全在「牌已在堆里」的上下文**，
没有任何一处用它凭空造牌 —— 说明它根本不是造牌 API。

不同场景的正确造牌入口：

| 场景 | 写法 |
|---|---|
| 战斗内造牌 | `combatState.CreateCard(canonicalCard, player)`（= `ToMutable + AddCard + AfterCreated`） |
| 战斗内生成到手牌 | `ToMutable()` + 设 `Owner` → `CardPileCmd.AddGeneratedCardToCombat(c, PileType.Hand, player, ...)` |
| 战斗外（事件/遗物）加入牌组 | `ToMutable()` + 设 `Owner` → `CardPileCmd.Add(c, PileType.Deck, CardPilePosition.Bottom, null, false)` |

**排查口诀**：界面操作没反应 → 先去日志搜 `CanonicalModelException` / `AssertMutable` /
`NullReferenceException` / `CardModel.CreateClone`。
这些异常发生在 `EventOption.Chosen()`、遗物触发等**流程回调里**，
会被框架的 `LogTaskExceptions` 吞进日志，玩家侧只看到「卡住」。

---

## 五、已知限制

- 部分卡图使用程序化生成的占位图（以墨色/金/玉等色系区分）。
  替换 `images/cards/{类名}.png` 即可。
- 先古之民为**复用原版**（Neow / Darv）的对话与选项，未制作独立的先古 NPC。
- 仅支持单人模式；多人联机的同步未做验证。
