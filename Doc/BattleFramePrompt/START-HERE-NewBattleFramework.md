# START HERE - 新战斗框架迁移说明

## 这份文件是干什么的

这份文件是给“新项目第一次打开时”使用的启动说明。

目的只有一个：

- 让 AI 在新的 `Unity + HybridCLR` 项目里，能快速接上当前老项目里已经完成的分析和规划；
- 避免新项目里丢失上下文，导致又从头解释一遍；
- 明确第一步该做什么，不要一上来就把旧战斗系统整包复制进去。

## 在新项目里应该一起复制过去的文件

请把下面 4 个文件一起复制到新项目中，建议放到：

```text
Docs/BattleMigration/
```

建议复制的文件：

- `NewBattleFramework-Boundaries.md`
- `NewBattleFramework-Prompt.md`
- `NewBattleFramework-MigrationSlices.md`
- `START-HERE-NewBattleFramework.md`

如果你不想建子目录，也可以直接放到新项目的 `Docs/` 根目录，但最好保持这 4 个文件在一起。

## 新项目里第一次对 AI 说什么

在新的 Unity + HybridCLR 项目里，打开对话后，直接发下面这段话：

```md
这是一个新的 Unity + HybridCLR 项目。

我已经把以下文档复制到当前项目中：
- `Docs/BattleMigration/NewBattleFramework-Boundaries.md`
- `Docs/BattleMigration/NewBattleFramework-Prompt.md`
- `Docs/BattleMigration/NewBattleFramework-MigrationSlices.md`
- `Docs/BattleMigration/START-HERE-NewBattleFramework.md`

请先完整阅读这些文档，然后按其中的约束继续工作。

当前目标：
1. 不要直接复制旧项目整套战斗代码
2. 先搭建新的战斗框架骨架
3. 按分阶段迁移思路，从“最小战斗闭环”开始
4. 优先实现：
   - BattleBootstrap
   - BattleWorld
   - FrameLoop
   - OrderBus
   - HeroSystem
   - AISystem
   - BulletSystem
   - StageHandler
   - VictoryRule
5. 暂时不要接入复杂 Buff、复杂词条、复杂活动玩法

开始前，先告诉我你准备创建哪些目录和文件。
```

## 新项目里的工作原则

进入新项目后，请始终遵守这些原则：

### 1. 不要整包复制旧战斗系统

旧项目战斗系统里已经积累了很多技术债，包括但不限于：

- 全局单例穿透
- 初始化顺序强依赖
- 大总管类职责过重
- 业务特例散落在基础实体层
- 指令字段语义较弱

所以新项目的目标不是“原样搬运”，而是：

- 保留核心思想
- 重建清晰边界
- 分阶段迁移能力

### 2. 先跑最小闭环，再加复杂能力

新项目第一阶段只应该做这些：

- 单英雄
- 单波怪
- 基础移动
- 基础锁敌
- 基础普通攻击
- 子弹命中
- 怪物死亡
- 胜利结算

暂时不要优先做：

- 复杂词条系统
- 复杂 Buff 联动
- 活动玩法
- 多种特殊副本
- 大量表现层特例

### 3. 先保证边界清晰，再谈迁移效率

优先保证：

- 模块职责明确
- 依赖方向单向
- 不依赖全局单例
- 核心循环独立
- 命令系统强类型化
- 热更边界清楚

不要为了图快，把旧项目里职责混乱的结构继续复制到新项目。

### 4. 先生成骨架，再补实现

在新项目里，建议按这个顺序推进：

1. 生成目录结构
2. 生成接口和骨架类
3. 跑通最小战斗闭环
4. 再逐步迁移旧系统能力

不要第一次就让 AI 生成“完整战斗系统全实现”。

## 新项目的推荐执行顺序

### 第一步：先读文档

必须先读：

- `NewBattleFramework-Boundaries.md`
- `NewBattleFramework-MigrationSlices.md`

其中：

- `Boundaries` 负责定义架构边界
- `MigrationSlices` 负责定义迁移顺序
- `Prompt` 负责辅助 AI 生成第一版骨架

### 第二步：生成第一版框架骨架

优先搭建：

- `BattleBootstrap`
- `BattleWorld`
- `FrameLoop`
- `BattleContext`
- `OrderBus`
- `FrameCommandBuffer`
- `HeroSystem`
- `AISystem`
- `BulletSystem`
- `WaveSystem`
- `StageHandler`
- `VictoryRule`

### 第三步：跑通最小闭环

最小闭环的验证标准：

- 英雄可移动
- 怪物可刷新
- 英雄可自动攻击
- 子弹可命中
- 怪物死亡后可结算
- 胜利规则可触发

### 第四步：再按切片迁移旧能力

推荐迁移顺序：

1. 角色移动/锁敌/普通攻击
2. 怪物刷新与基础 AI
3. 子弹与命中链
4. Buff
5. 词条
6. 玩法模式与关卡特化
7. 回放/补帧

## 如果新项目里需要继续让我直接落代码

你在新项目里可以继续这样对我说：

```md
请基于当前项目中的 BattleMigration 文档，先搭建第一版战斗框架骨架。

要求：
- 不要直接复制旧项目代码
- 先按最小闭环做
- 先创建目录和核心骨架类
- 创建前先列出计划创建的文件
- 创建后说明每个文件的职责
```

如果你已经生成了一版骨架，还可以继续这样说：

```md
请继续基于当前骨架，开始做第一个迁移切片：
角色移动、基础锁敌、普通攻击。

要求：
- 保持架构边界不被破坏
- 不要引入全局单例
- 不要提前接入复杂玩法特例
```

## 最后提醒

新项目里最容易犯的错误有 3 个：

1. 直接复制旧战斗目录过去，然后再慢慢删
2. 先做复杂玩法，而不是先做最小闭环
3. 没有把上下文文档带过去，导致每次都重新解释一遍

正确方式是：

- 把这 4 份文档带过去
- 先生成骨架
- 先做最小闭环
- 再按切片逐步迁移

这样新项目才能真正得到一套“优化后的新战斗系统”，而不是旧系统的技术债复制品。
