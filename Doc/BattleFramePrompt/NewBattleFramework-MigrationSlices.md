# 新战斗系统分阶段迁移切片清单

## 目的

这份文档用于把旧战斗系统迁移到新框架时，拆成一组**可独立开发、可独立验证、可独立回归**的纵向切片。

原则：

- 每次只迁一个最小闭环。
- 每个切片必须可运行、可验证、可回退。
- 不允许先全量搬代码再集中联调。

## 总体迁移顺序

1. 最小战斗闭环
2. 输入命令与帧缓冲
3. 角色移动与基础锁敌
4. 怪物刷新与基础 AI
5. 子弹与命中链
6. 伤害结算与死亡链
7. Buff 系统
8. 词条系统
9. 玩法模式
10. 关卡规则与胜负规则
11. 回放与补帧
12. 旧配置适配与业务玩法迁移

## 切片 0：框架骨架搭建

### 目标

在新项目中搭出战斗框架的目录结构、接口和最小调度骨架。

### 对应旧系统参考

- `Assets/Script/CSharp/Scene/FightScene/LDFightScene.cs`
- `Assets/Script/CSharp/Scene/FightScene/LDFightFrameCtrl.cs`
- `Assets/Script/CSharp/Fight/LDCtrollerOrder.cs`

### 新系统应落地的模块

- `BattleBootstrap`
- `BattleWorld`
- `FrameLoop`
- `BattleContext`
- `OrderBus`
- `EntityRegistry`
- `IPlayMode`
- `IStageHandler`
- `IVictoryRule`

### 验收标准

- 能成功进入一个空白战斗场景。
- `FrameLoop` 可稳定推进固定逻辑帧。
- `BattleWorld` 可按顺序调用各系统的 `Tick`。

## 切片 1：最小战斗闭环

### 目标

先跑通一个最小可玩闭环：

- 单英雄
- 单波怪
- 普通移动
- 自动锁敌
- 普通射击
- 怪物死亡
- 胜利结算

### 对应旧系统参考

- `Assets/Script/CSharp/Scene/FightNormal/LDFightNormalScene.cs`
- `Assets/Script/CSharp/Fight/Fight/RoleMgr/LDRoleMgr.cs`
- `Assets/Script/CSharp/Fight/Fight/Wave/LDWaveMgr.cs`
- `Assets/Script/CSharp/Fight/Fight/Bullet/Emitter/LDHeroBulletEmitter.cs`

### 新系统建议模块

- `HeroSystem`
- `AISystem`
- `WaveSystem`
- `BulletSystem`
- `DefaultStageHandler`
- `KillAllVictoryRule`

### 验收标准

- 玩家可以移动。
- 敌人可以刷新。
- 英雄可自动攻击。
- 子弹命中后怪物掉血并死亡。
- 杀光怪物后触发胜利。

## 切片 2：命令系统替换输入直驱

### 目标

把本地输入统一变成强类型命令，再按帧消费。

### 对应旧系统参考

- `Assets/Script/CSharp/Fight/LDCtrollerOrder.cs`
- `Assets/Script/CSharp/Fight/Fight/Soldier/Role/LDMainRole.cs`

### 新系统建议模块

- `IFrameCommand`
- `MoveCommand`
- `UseSkillCommand`
- `SelectTraitCommand`
- `FrameCommandBuffer`
- `OrderBus`

### 关键要求

- 命令结构必须强类型化。
- 不使用 `PM1/PM2/LV/SQ` 风格字段。
- 命令消费应与固定逻辑帧强绑定。

### 验收标准

- 输入不再直接改角色状态。
- 命令可按帧存入和读出。
- 命令消费结果稳定可追踪。

## 切片 3：角色移动与锁敌

### 目标

把旧系统里英雄移动、面向、锁敌拆成清晰的英雄系统能力。

### 对应旧系统参考

- `Assets/Script/CSharp/Fight/Fight/Soldier/Hero/LDHeroPlayer.cs`
- `Assets/Script/CSharp/Fight/Fight/Soldier/Role/LDMainRole.cs`

### 新系统建议模块

- `HeroEntity`
- `HeroMovementController`
- `HeroTargetingService`
- `HeroStateController`

### 关键要求

- 移动、锁敌、朝向、状态切换分离。
- Hero 不直接感知玩法规则。
- 锁敌逻辑可替换。

### 验收标准

- 英雄移动输入可被命令系统驱动。
- 英雄可根据范围/优先级锁定目标。
- 英雄面向与攻击目标同步。

## 切片 4：怪物刷新与基础 AI

### 目标

先迁最简单的怪物：

- 刷新
- 待机
- 追击
- 攻击
- 死亡

### 对应旧系统参考

- `Assets/Script/CSharp/Fight/Fight/Wave/LDWaveMgr.cs`
- `Assets/Script/CSharp/Fight/Fight/AI/LDNormalAI.cs`
- `Assets/Script/CSharp/Fight/Fight/AI/State/LDAIBaseState.cs`
- `Assets/Script/CSharp/Fight/Fight/AI/State/LDAIIdleState.cs`
- `Assets/Script/CSharp/Fight/Fight/AI/State/LDAIPursueState.cs`

### 新系统建议模块

- `WaveSystem`
- `SpawnSystem`
- `AIEntity`
- `AIStateController`
- `AIIdleState`
- `AIPursueState`
- `AIAttackAbility`

### 关键要求

- AI 状态切换必须可追踪。
- 锁敌逻辑不要硬编码在怪物实体内部。
- NavMesh、直线追击、技能执行要能解耦。

### 验收标准

- 怪物可刷新并进入 Idle。
- 看到目标后切 Pursue。
- 到攻击距离后执行攻击。
- 血量归零后进入死亡回收流程。

## 切片 5：子弹与命中链

### 目标

把“发射 -> 飞行 -> 命中 -> 伤害触发”这一链跑通。

### 对应旧系统参考

- `Assets/Script/CSharp/Fight/Fight/Bullet/Emitter/LDHeroBulletEmitter.cs`
- `Assets/Script/CSharp/Fight/Fight/Bullet/Base/LDTrackBullet.cs`

### 新系统建议模块

- `WeaponFireService`
- `BulletFactory`
- `BulletSystem`
- `HitResolver`

### 关键要求

- 发射逻辑与子弹构建逻辑拆分。
- 子弹配置与命中处理分离。
- 命中链必须可插入 Buff/Trait 修正。

### 验收标准

- 子弹可正常发射。
- 子弹可飞行并命中目标。
- 命中后能产生伤害请求。

## 切片 6：伤害、受击、死亡链

### 目标

明确从“命中事件”到“血量变化”到“死亡结算”的统一链路。

### 对应旧系统参考

- `Assets/Script/CSharp/Fight/Fight/Soldier/Hero/LDHeroPlayer.cs`
- 旧系统中的 `DamageCalcUtil`、受击入口、击退状态相关代码

### 新系统建议模块

- `DamageService`
- `DamageContext`
- `HitReactionService`
- `DeathService`

### 关键要求

- 伤害计算与表现反应分开。
- 死亡结算统一出口。
- 击退、受击硬直、无敌判定独立于纯伤害计算。

### 验收标准

- 英雄与怪物都能共用一套伤害入口。
- 死亡时能正确回收、广播、结算。

## 切片 7：Buff 系统

### 目标

先迁基础 Buff 运行时，支持最小的增删改查与 Tick。

### 对应旧系统参考

- 旧系统 Buff 根管理器和 Hero/AI Buff 管理器

### 新系统建议模块

- `BuffSystem`
- `IBuff`
- `BuffRuntime`
- `BuffRegistry`

### 关键要求

- Buff 运行时必须与实体隔离。
- Buff 的 Tick、Add、Remove、Refresh 要统一。
- Buff 变化必须可追踪。

### 验收标准

- 支持给 Hero/AI 添加持续性 Buff。
- 支持定时到期与移除。
- 支持基础属性修改。

## 切片 8：词条系统

### 目标

在新框架中建立词条系统作为独立一等公民。

### 对应旧系统参考

- `Assets/Script/CSharp/Fight/Fight/CiTiao/LDCiTiaoMgr.cs`

### 新系统建议模块

- `TraitSystem`
- `ITrait`
- `TraitRuntime`
- `TraitSelectionCommand`

### 关键要求

- 词条不直接塞进实体类里。
- 词条应通过事件、数值修正、能力注册影响战斗。
- 词条选择要能被命令系统记录和回放。

### 验收标准

- 支持在战斗中选择基础词条。
- 词条能影响攻击、Buff、子弹或属性中的至少一个维度。

## 切片 9：玩法模式与关卡规则

### 目标

把旧项目中大量散落逻辑，统一收敛到玩法规则和关卡规则层。

### 对应旧系统参考

- `Assets/Script/CSharp/Fight/PlayMode/LDPlayModeBase.cs`
- `Assets/Script/CSharp/Fight/Fight/PassHandler/Base/LDPassHandler.cs`
- `Assets/Script/CSharp/Fight/Fight/VictoryCondition/LDVictoryBase.cs`

### 新系统建议模块

- `IPlayMode`
- `IStageHandler`
- `IVictoryRule`
- `DefaultPlayMode`
- `DefaultStageHandler`
- 若干特化规则实现

### 关键要求

- 玩法差异不写进 Hero/AI/Bullet。
- 关卡差异不写进基础系统。
- 胜利条件独立于玩法逻辑。

### 验收标准

- 至少支持一种默认玩法和一种特化玩法。
- 至少支持一种默认关卡规则和一种特化关卡规则。

## 切片 10：回放与补帧

### 目标

把旧系统最有价值的“指令重放能力”恢复到新框架。

### 对应旧系统参考

- `Assets/Script/CSharp/Fight/LDCtrollerOrder.cs`

### 新系统建议模块

- `ReplayService`
- `ReplayRecord`
- `ReplayFrameData`
- `MissFrameRequest`

### 关键要求

- 录制的是命令帧，不是表现结果。
- 回放使用与正常战斗一致的命令消费链。
- 补帧请求与命令缓冲分离。

### 验收标准

- 一局最小战斗可以录制。
- 可从录制文件回放。
- 缺帧时可发起补帧请求接口。

## 切片 11：旧数据适配

### 目标

先让新框架能读旧配置，再谈迁旧逻辑。

### 新系统建议模块

- `OldPassDataAdapter`
- `OldRoleDataAdapter`
- `OldTraitAdapter`
- `OldBuffConfigAdapter`

### 关键要求

- 先适配数据，不直接复制旧业务实现。
- 尽量把旧配置映射到新领域模型。

### 验收标准

- 新战斗最小闭环可以吃旧项目的一部分配置。

## 切片 12：业务玩法按域迁移

### 目标

按玩法域逐步迁入旧项目复杂玩法，而不是一次性混装。

### 推荐顺序

1. 普通 PVE
2. 精英/Boss 关
3. 特殊波次与特殊胜利条件
4. 词条强化型玩法
5. 公会/赛季/活动副本

### 原则

- 每迁一个玩法域，就补一组回归样例。
- 每迁一个玩法域，就补一组性能观察点。
- 每迁一个玩法域，就确认它落在 `PlayMode`、`StageHandler`、`VictoryRule` 的哪个层。

## 每个切片都要有的交付物

- 一份模块边界说明。
- 一份最小流程图。
- 一组可验证场景。
- 一份日志点清单。
- 一份回归检查单。

## 推荐执行节奏

### 第 1 周

- 切片 0
- 切片 1
- 切片 2

### 第 2 周

- 切片 3
- 切片 4
- 切片 5

### 第 3 周

- 切片 6
- 切片 7
- 切片 8

### 第 4 周

- 切片 9
- 切片 10
- 切片 11

### 第 5 周及以后

- 切片 12，按玩法域持续迁移

## 迁移过程中的禁止事项

- 不要先全量复制旧目录再试图清理。
- 不要在新项目里继续复制 `Global.gApp` 模式。
- 不要先迁活动玩法再补基础链路。
- 不要跳过命令系统直接让输入改角色状态。
- 不要把旧项目所有特例都直接写进新实体类。

## 最终成功标准

- 新框架能独立支撑最小战斗闭环。
- 新框架能逐步接住旧项目玩法，而不是被旧项目结构反向污染。
- 新框架比旧框架更容易理解、调试、扩展和热更。
