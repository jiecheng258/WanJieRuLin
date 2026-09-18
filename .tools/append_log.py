# -*- coding: utf-8 -*-
"""追加本轮 9 项 bug 修复记录到工作日志。"""
import io, os

LOG = r'C:\Users\wangx\WorkBuddy\2026-09-15-20-17-56\.workbuddy\memory\2026-09-15.md'

TEXT = """
---

## 续：9 项 bug 全量修复（本轮，最重要）

用户一次性提了 9 个 bug + 关键澄清「**鬼气是类似星辉的机制**」。
这句话推翻了之前 `PersistencePolicy.Run` 的设计 —— 鬼气应在**战斗内跨回合保留、
换战斗重置为 1**。

### 核心 API 突破：可打出条件的正确扩展点

**这是本轮最大的技术突破，之前一直搞错。**

- 原版 `CardModel.CanPlay(out UnplayableReason, out AbstractModel)` **没有 virtual**，
  无法重写（`CS0506`）。
- 反射确认真实扩展点：
  ```
  CardModelCapabilityPatches+IsPlayablePatch
      Void Postfix(MegaCrit.Sts2.Core.Models.CardModel, Boolean ByRef)
  ```
  它 patch 的是 `protected virtual bool IsPlayable`，Postfix 里调用
  `CardModelCapabilityHost.ApplyCanPlay(card, value)`，
  后者再转发到所有 `ICardPlayStateContributor.CanPlay(card)`。
- **⇒ 结论：只要实现 `ICardPlayStateContributor.CanPlay` 就够了，
  完全不需要（也不能）重写原版 `CanPlay`。**
  之前判断「RitsuLib 的 CanPlay 没被消费」是**错的**。

### 各 bug 根因与修法

| # | 根因 | 修法 |
|---|---|---|
| 1/5 | `gen_loc.py` 的 CARDS 里手写了「升级后…」；`build_cards()` 把同一串写进 `.description` 与 `.smartDescription` | 删除全部此类句子，改由 `{Var:diff()}` 渲染升级值 |
| 2 | `TurnStartPolicy.None` + `PersistencePolicy.Run` | `PersistencePolicy.Combat` + `DefaultAmount=1` + `BeforeCombatStart` 里显式 `GhostQi.Set(player,1)`（新增 `WanJieRuLinRelic.ResetGhostQiToCombatStart()`） |
| 3 | `ICardPlayStateContributor` 被认为没接线，于是试图重写 `CanPlay` → 编译失败 | 见上方突破：只留接口实现即可，删掉 `CanPlay` override |
| 4 | ①起始牌组计数问题 ②X 费用是**必需支付**，0 鬼气时牌被灰掉 | ①`绘 ×2` ②`SetGhostQiCostX` 改用 `SecondaryCosts().SetAllowingShortfall(id, SecondaryResourceCost.X(m))` ③`GhostQiXValue` 加 `ledger.Spent()` 兜底 |
| 6 | `Swords` 用 `new JinJianPower()` —— 新建实例没有 ModelId，框架叠加/hook 找不到它 | 改用 `ModelDb.Get(Type)` / `ModelDb.Power<T>()` 取规范实例 |
| 7 | `FreePlayBindingRegistry` 用错时机（出牌**后**才标记）；`_firstCardWasAttack` 是死代码 | 改为**出牌前**的检测器：`FreePlayBindingRegistry.Register(id, play => already >= 2 && HasGuiYu(owner))`，静态 `Dictionary<Player,int> PlayedThisTurn` 计数；第 4 张用 `BaseReplayCount += 1` |
| 8 | 裸用原版 `TemporaryStrengthPower` / `TemporaryDexterityPower` —— 它们是**包装型**能力，需 `OriginModel` + `InternallyAppliedPower`，裸用会半初始化卡住出牌流程 | 新建 `WanJieTempStrengthPower` / `WanJieTempDexterityPower : ModTemporaryPowerTemplate` 暴露两者，6 处调用点全换 |
| 9 | **RitsuLib 的次要资源费用不会自动渲染进卡面描述**，自定义资源尤其看不见 | 新增自动注入：`parse_ghost_qi_costs()` 从卡牌源码解析真实 `SetGhostQiCost/X`，`build_cards()` 把「消耗 N 点鬼气」插到描述首行 —— **22 张收费卡 100% 覆盖，永不漂移** |

### 额外发现
- **Replay 不是 `CardKeyword`**（枚举只有 None/Exhaust/Ethereal/Innate/Unplayable/Retain/Sly/Eternal）。
  重放 = `CardModel.BaseReplayCount`（可直接 +1）。
- `ModTemporaryPowerTemplate` 的 `OriginModel` / `InternallyAppliedPower` 是
  `public override` 属性，必须自己给值。
- `GuiYuPower.PlayedThisTurn` 是静态字典，加了 `BeforeCombatStart` 清理，防跨战斗串味。

### 新增/改写的自建工具
- `.tools/audit_placeholders.py` —— 校验每张卡 `CanonicalVars` 里的变量与
  `zhs/cards.json` 占位符一一对应，并扫「升级后/Upgraded」残留
- `.tools/audit_ghostqi.py` —— 校验「代码收的鬼气」与「文案写的鬼气」一致
- `.tools/gen_loc.py` —— 新增 `parse_ghost_qi_costs()` / `ghost_qi_cost_line()` 注入逻辑
- `.tools/gen_art.py` —— POWERS 表补 `WanJieTempStrengthPower` / `WanJieTempDexterityPower` 两张图
- `.tools/apipeek2/` —— 反射探针重写。**坑**：RitsuLib 的实际程序集名是
  `STS2-RitsuLib.Runtime.dll`（不是 `STS2-RitsuLib.dll`），
  且 `Program.cs` 里用 `typeof(ModCardTemplate)` 会触发依赖解析失败，
  必须用 `AppDomain.AssemblyResolve` + 按去连字符/下划线模糊匹配文件名。

### 本轮结果
```
dotnet build WanJieRuLin.sln -c Debug  ->  0 错误 / 0 警告
PCK 13,865,616 B
审计：卡牌 52 缺本地化 0 缺卡图 0 / 遗物 5 / 能力 25 有问题 0
已自动部署到游戏 mods/WanJieRuLin/（23:11）
打包 WanJieRuLin-v0.1.0-install.zip (13.1 MB) + source.zip (16.3 MB)
```

### 重要路径修正
`WanJieRuLinCode` 是 **`WanJieRuLin` 的兄弟目录**，不是子目录！
```
C:\\Users\\wangx\\Documents\\Default Project\\WanJieRuLin\\
   |- WanJieRuLin\\          <- Godot 项目（images / localization / scenes）
   |- WanJieRuLinCode\\      <- C# 源码（兄弟！）
```
写脚本时别再 `os.path.join(ROOT, 'WanJieRuLinCode')`。
"""

with io.open(LOG, 'a', encoding='utf-8', newline='\n') as f:
    f.write(TEXT)

print('appended, new size =', os.path.getsize(LOG))
