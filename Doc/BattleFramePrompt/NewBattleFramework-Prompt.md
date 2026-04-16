# 新项目战斗框架生成 Prompt

## 使用方式

把下面的 Prompt 原样复制到新的 `Unity + HybridCLR` 项目里，作为 AI 辅助生成战斗框架骨架的输入。

建议先让 AI 只生成目录结构、接口、核心骨架类和注释，不要一次生成大量业务逻辑。

---

## Prompt 正文

你现在是一个资深 Unity 架构工程师，请为一个新的 `Unity + C# + HybridCLR` 项目设计并生成一套“可维护、可扩展、支持后续热更玩法”的战斗框架骨架。

### 一、项目目标

请搭建一套新的战斗框架，满足以下目标：

1. 使用固定逻辑帧更新战斗。
2. 输入必须先转成命令，再按帧消费。
3. 预留网络帧同步、补帧、录像回放能力。
4. 支持玩法扩展、关卡扩展、胜负规则扩展。
5. 支持角色、AI、子弹、Buff、词条、波次等子系统独立演进。
6. 战斗核心尽量与 UI、资源加载、业务系统、热更规则解耦。
7. 适配 `HybridCLR`，但不要让高频核心循环严重依赖热更层。

### 二、架构要求

请严格按以下分层设计：

- `Bootstrap`：负责战斗启动与销毁。
- `Core`：负责 `BattleWorld`、`BattleContext`、`FrameLoop`、时间推进。
- `Commands`：负责命令定义、按帧缓冲、命令总线。
- `Entities`：负责 Hero、AI、Bullet、Buff、Trait、Wave 等领域对象与系统。
- `Rules`：负责 `PlayMode`、`StageHandler`、`VictoryRule`。
- `Services`：负责 Replay、Network、Config、EventBus、DebugTrace。
- `Presentation`：负责 Camera、VFX、HUD，不允许反向改逻辑状态。

### 三、目录结构要求

请生成如下目录结构及占位类：

```text
Assets/
  Game/
    Battle/
      Runtime/
        Bootstrap/
        Core/
        Commands/
          Commands/
        Entities/
          Hero/
          AI/
          Bullet/
          Buff/
          Trait/
          Wave/
          Element/
        Rules/
          PlayModes/
          StageHandlers/
          VictoryRules/
        Services/
          Replay/
          Network/
          Config/
          Events/
          DebugTrace/
        Presentation/
          Camera/
          Vfx/
          Hud/
      Hotfix/
        BattleRules/
        Traits/
        Buffs/
        PlayModes/
```

### 四、必须生成的核心类

请至少生成以下类或接口的骨架：

#### Bootstrap

- `BattleBootstrap`

#### Core

- `BattleWorld`
- `BattleContext`
- `FrameLoop`
- `BattleTime`

#### Commands

- `IFrameCommand`
- `FrameCommandBuffer`
- `OrderBus`
- `MoveCommand`
- `UseSkillCommand`
- `SelectTraitCommand`

#### Entities

- `EntityRegistry`
- `HeroSystem`
- `AISystem`
- `BulletSystem`
- `BuffSystem`
- `TraitSystem`
- `WaveSystem`
- `HeroEntity`
- `AIEntity`
- `BulletEntity`

#### Rules

- `IPlayMode`
- `IStageHandler`
- `IVictoryRule`
- `DefaultPlayMode`
- `DefaultStageHandler`
- `KillAllVictoryRule`

#### Services

- `IReplayService`
- `INetAdapter`
- `IConfigProvider`
- `IEventBus`
- `IDebugTraceService`
- `LocalReplayService`
- `NullNetAdapter`
- `InMemoryEventBus`

### 五、编码约束

请严格遵守以下约束：

1. 不要引入 `Global.gApp` 一类全局单例。
2. 不要让 `BattleWorld` 变成超大上帝类。
3. 不要使用 `PM1/PM2/LV/SQ` 这种弱语义字段来表达命令。
4. 命令必须使用强类型结构。
5. 不要把玩法特例写进 Hero/AI/Bullet 实体类。
6. 玩法和关卡差异必须通过 `PlayMode`、`StageHandler`、`VictoryRule` 扩展。
7. 高频核心循环代码与热更层要有清晰边界。
8. 每个系统类都要写清楚职责注释。
9. 所有核心类应尽量面向接口编程。
10. 预留日志和调试追踪接口，至少能追踪帧号、状态切换、命令消费、Buff 增删、伤害链。

### 六、逻辑帧要求

请实现一个固定逻辑帧结构，满足：

- 渲染更新与逻辑更新分离。
- 逻辑更新使用固定步长，例如 `1 / 30f` 或 `1 / 33f`。
- `FrameLoop` 提供：
  - 启动
  - 暂停
  - 恢复
  - Tick
  - 固定帧推进
- `BattleWorld` 的逻辑更新顺序至少包含：
  1. 命令消费
  2. HeroSystem
  3. AISystem
  4. BulletSystem
  5. BuffSystem
  6. TraitSystem
  7. WaveSystem
  8. StageHandler
  9. VictoryRule

### 七、命令系统要求

请实现命令系统，满足：

- 本地输入先转换为命令对象。
- 命令按帧写入 `FrameCommandBuffer`。
- `OrderBus` 可以：
  - 推送本地命令
  - 接收远端命令
  - 按帧取命令
  - 校验缺帧
  - 预留补帧接口
- `ReplayService` 预留录制与回放能力。

### 八、扩展机制要求

请把以下能力设计成可扩展：

- 新玩法模式：通过新增 `IPlayMode` 实现类扩展。
- 新关卡逻辑：通过新增 `IStageHandler` 实现类扩展。
- 新胜负规则：通过新增 `IVictoryRule` 实现类扩展。
- 新词条：通过 `TraitSystem` 的注册机制扩展。
- 新 Buff：通过 `BuffSystem` 的注册机制扩展。

### 九、HybridCLR 边界要求

请按以下原则预留热更边界：

- 稳定层：
  - `BattleWorld`
  - `FrameLoop`
  - 命令系统
  - 核心实体结构
- 热更层：
  - 玩法规则
  - 关卡规则
  - 特殊 Buff
  - 特殊词条
  - 活动玩法

生成代码时请在适当位置加注释说明：

- 哪些代码建议放稳定程序集
- 哪些代码建议放热更程序集

### 十、输出要求

请按以下顺序输出：

1. 先给出整体目录树。
2. 再给出核心类职责说明。
3. 再生成最小可运行骨架代码。
4. 每次只生成骨架，不要一次生成复杂业务逻辑。
5. 代码必须可读、命名清晰、注释简洁。

### 十一、最小可运行目标

请以“最小可运行战斗切片”为目标，先支持：

- 单英雄
- 单波怪
- 基础锁敌
- 基础射击
- 子弹命中
- 扣血
- 怪物死亡
- 胜利判定

不要在第一版里加入：

- 复杂 Buff 联动
- 复杂词条联动
- 多玩法混搭
- 多人联机完整实现
- 活动特例逻辑

### 十二、代码风格要求

- C# 命名遵循统一 PascalCase。
- 每个类文件只放一个主要类型。
- 每个系统类都写职责注释。
- 所有接口和骨架类优先可扩展，不要过早写死业务细节。
- 对未来迁移旧项目代码要友好，必要时可加 `Adapter` 占位类。

---

## 推荐追加指令

如果你已经用上面的 Prompt 生成了第一版骨架，可以继续追加这段指令：

请基于当前已生成的战斗框架，继续补全以下内容，但仍然保持“最小闭环优先”原则：

1. 增加 `OldPassDataAdapter`、`OldRoleDataAdapter`、`OldTraitAdapter` 占位结构。
2. 为 `IDebugTraceService` 增加状态切换、命令消费、伤害结算的调试接口。
3. 为 `ReplayService` 增加录制帧命令和回放帧命令的基础数据结构。
4. 给 `HeroSystem`、`AISystem`、`BulletSystem` 增加最小的 `Update` 逻辑示例。
5. 不要引入任何全局单例。

## 使用建议

- 第一次只让 AI 生成骨架和接口，不要让它直接“把整个老战斗系统翻译过来”。
- 生成后先人工检查目录、职责、依赖方向是否正确。
- 确认边界清晰后，再逐步迁移旧项目逻辑。
