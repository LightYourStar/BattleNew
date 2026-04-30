# BattleNew 战斗系统技术文档

> **版本**: 2026.04 | **引擎**: Unity + C# | **热更方案**: HybridCLR  
> **代码量**: 90 个 C# 文件, 5267 行 | **架构风格**: ECS-like, 接口驱动  
> **核心特性**: 固定帧 30FPS · 确定性随机 · 完整回放 · 全配置驱动 · 支持热更

---

## 目录

1. [项目概览](#1-项目概览)
2. [分层架构](#2-分层架构)
3. [核心循环: BattleWorld & FrameLoop](#3-核心循环-battleworld--frameloop)
4. [战斗上下文: BattleContext](#4-战斗上下文-battlecontext)
5. [实体系统](#5-实体系统)
6. [命令系统](#6-命令系统)
7. [伤害管线](#7-伤害管线)
8. [Buff 与词条系统](#8-buff-与词条系统)
9. [波次与刷怪系统](#9-波次与刷怪系统)
10. [AI 系统](#10-ai-系统)
11. [规则层: 玩法模式与胜负](#11-规则层-玩法模式与胜负)
12. [回放系统](#12-回放系统)
13. [配置系统](#13-配置系统)
14. [Hotfix 热更层](#14-hotfix-热更层)
15. [扩展指南](#15-扩展指南)

---

## 1. 项目概览

BattleNew 是一个用于 Unity 的**固定帧战斗框架**，设计目标是构建一个：

- **确定性可复现**：给定相同输入（RngSeed + 命令帧序列），输出完全一致
- **可热更**：通过 HybridCLR + Hotfix Assembly，在不更新包体的情况下扩展玩法、Buff、词条
- **可配置**：策划通过 Excel 表驱动所有数值，程序不硬编码
- **可测试**：核心逻辑不依赖 Unity MonoBehaviour，纯 C# 可单元测试

### 1.1 双 Assembly 策略

```
Game.Battle.Runtime  (Runtime Assembly)     ← 不依赖 Unity 特定功能, 纯逻辑
Game.Battle.Hotfix   (Hotfix Assembly)      ← 可热更, 依赖 Runtime
```

- **Runtime** 定义接口和基础实现（如 `DefaultPlayMode`、`KillAllVictoryRule`）
- **Hotfix** 注册具体内容（如 `DamageBoostBuff`、`ElitePlayMode`、`DamageBoostTrait`）
- Hotfix 通过 `setupContext` 回调钩入 Runtime，Runtime 层完全不感知 Hotfix 的存在

---

## 2. 分层架构

```
┌──────────────────────────────────────────────────┐
│  引导层 (Bootstrap)                                │
│  BattleBootstrap · BattleRunnerBehaviour          │  ← 外部入口, 生命周期管理
├──────────────────────────────────────────────────┤
│  编排层 (Orchestration)                            │
│  BattleWorld → 按序调度各系统 Tick                 │  ← 纯粹的调度者, 无业务逻辑
├──────────────────────────────────────────────────┤
│  上下文层 (Context)                                │
│  BattleContext → 聚合所有运行时状态                │  ← 时间/随机/实体/系统/服务
├──────────────────────────────────────────────────┤
│  实体层 (Entities)                                 │
│  Hero · AI(Enemy) · Bullet · Buff · Trait · Wave  │  ← 游戏对象的核心数据与行为
├──────────────────────────────────────────────────┤
│  规则层 (Rules)                                    │
│  PlayMode · StageHandler · VictoryRule            │  ← 玩法/关卡/胜负判定
├──────────────────────────────────────────────────┤
│  服务层 (Services)                                 │
│  Replay · Network · EventBus · DebugTrace · Config│  ← 横切关注点, 跨系统共享
└──────────────────────────────────────────────────┘
```

**依赖方向**: 上层依赖下层，下层不感知上层。通过接口 (`INetAdapter`, `IReplayService`, `IEventBus`, `IConfigProvider`) 实现依赖反转。

---

## 3. 核心循环: BattleWorld & FrameLoop

### 3.1 FrameLoop — 固定帧循环

这是整个战斗系统的**心跳**。它把不稳定的渲染帧 (可变 deltaTime) 切成稳定的逻辑帧 (固定 1/30s)。

```csharp
public sealed class FrameLoop
{
    private float _accumulator;
    public float FixedStep { get; }       // 1/30 = 0.0333s
    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }
    public Action<float>? OnFixedTick;    // 逻辑帧回调

    public void Tick(float deltaTime)
    {
        if (!IsRunning || IsPaused) return;
        _accumulator += deltaTime;
        while (_accumulator >= FixedStep)
        {
            OnFixedTick?.Invoke(FixedStep);  // 触发逻辑帧
            _accumulator -= FixedStep;
        }
    }
}
```

### 3.2 BattleWorld — 编排者

`BattleWorld` 不包含任何业务逻辑，它的唯一职责是**按顺序调用各系统 Tick**。

```csharp
// BattleWorld 每帧调度顺序 (简化)
void Tick(float dt)
{
    Context.HeroMovementController.ApplyMove(...);
    Context.HeroTargetingService.FindNearestAliveEnemy(...);
    Context.HeroStateController.UpdateState(...);
    Context.WeaponFireService.TryFire(...);
    Context.AISystem.Tick(ctx, dt);
    Context.BulletSystem.Tick(ctx, dt);
    Context.BuffSystem.Tick(ctx, dt);
    Context.TraitSystem.Tick(ctx, dt);
    Context.WaveSystem.Tick(ctx, dt);
    Context.HitReactionService.Tick(ctx, dt);     // 击退衰减/硬直计时
    Context.DeathService.OnEntityDeath(...);
}
```

### 3.3 生命周期

```
EnterBattle(loadout, setupContext)
  → 创建 BattleWorld + BattleContext
  → setupContext 回调 (Hotfix 注入 HeroDef/WeaponDef/Trait)
  → SeedFromLoadout (根据 Loadout 创建英雄 + 武器)
  → FrameLoop.Start()

每帧:
  → FrameLoop.Tick(deltaTime)  → 触发 N 个逻辑帧 → BattleWorld 调度系统

退出:
  → FrameLoop.Stop()
  → PlayMode.OnBattleEnd(isVictory)
```

---

## 4. 战斗上下文: BattleContext

`BattleContext` 是战斗的**数据中心**，所有系统通过它访问共享状态。

```csharp
public sealed class BattleContext
{
    // ─── 基础设施 ───
    public BattleTime Time { get; }                 // 战斗时间轴
    public EntityRegistry Registry { get; }         // Hero/Enemy/Bullet 列表
    public IReplayService ReplayService { get; }
    public INetAdapter NetAdapter { get; }
    public IEventBus EventBus { get; }
    public IConfigProvider? ConfigProvider { get; }
    public IDebugTraceService DebugTraceService { get; }

    // ─── 确定性随机 ───
    public BattleRng Rng { get; }                   // 所有随机必须由此产生
    
    // ─── 配置单 (战前数据) ───
    public BattleLoadout Loadout { get; }           // 英雄/武器/词条池/RngSeed

    // ─── 命令系统 ───
    public OrderBus OrderBus { get; }

    // ─── 数据注册表 ───
    public HeroDefRegistry HeroDefRegistry { get; }
    public WeaponDefRegistry WeaponDefRegistry { get; }
    public TraitPoolRegistry TraitPoolRegistry { get; }

    // ─── 各系统实例 ───
    public HeroSystem HeroSystem { get; }
    public HeroMovementController HeroMovementController { get; }
    public HeroTargetingService HeroTargetingService { get; }
    public HeroStateController HeroStateController { get; }
    public WeaponFireService WeaponFireService { get; }
    public AISystem AISystem { get; }
    public BulletSystem BulletSystem { get; }
    public BuffSystem BuffSystem { get; }
    public TraitSystem TraitSystem { get; }
    public WaveSystem WaveSystem { get; }
    public SpawnSystem SpawnSystem { get; }
}
```

### 4.1 BattleLoadout — 战前配置单

`BattleLoadout` 携带进入战斗时的配置：英雄选择、武器选择、词条池、随机种子。它在录制和回放时保持一致。

```csharp
public sealed class BattleLoadout
{
    public string? HeroDefId { get; set; }          // 英雄 ID
    public string? WeaponDefId { get; set; }         // 武器 ID
    public string[] TraitPoolIds { get; set; }       // 本局词条池
    public long RngSeed { get; set; }                // 确定性随机种子
}
```

---

## 5. 实体系统

战斗中有三类实体，统一在 `EntityRegistry` 中管理。

### 5.1 实体类型总览

| 实体 | 类 | 管理方式 |
|------|-----|---------|
| 英雄 | `HeroEntity` | 固定数量 (通常 1 个), 开局播种 |
| 敌人 | `AIEntity` | 由 WaveSystem / SpawnSystem 动态生成 |
| 子弹 | `BulletEntity` | 由 WeaponFireService 发射, BulletSystem 回收 |

### 5.2 HeroEntity

```csharp
public sealed class HeroEntity
{
    public string Id { get; }                    // 运行时唯一 ID
    public Vector3 Position { get; set; }
    public Vector3 FacingDirection { get; set; }
    public float MaxHp { get; set; }
    public float CurrentHp { get; set; }
    public bool IsAlive => CurrentHp > 0f;
    public float MoveSpeed { get; set; }
    public float AttackRange { get; set; }
    public WeaponRuntime? CurrentWeapon { get; set; }
    public string? LockedTargetId { get; set; }
    public Vector3 KnockbackVelocity { get; set; }     // 击退速度
    public float StunRemaining { get; set; }            // 硬直剩余时间
    public float InvincibleRemaining { get; set; }      // 无敌帧剩余时间
}
```

### 5.3 英雄子系统 (拆分设计)

英雄的能力被拆分为独立的单职责类：

| 子系统 | 职责 |
|--------|------|
| `HeroMovementController` | 位置推进 + 朝向更新 |
| `HeroTargetingService` | 在攻击范围内选择最近存活敌人 |
| `HeroStateController` | 移动/攻击/待机状态切换 |
| `WeaponFireService` | 射击时机 + 枪口计算 + 子弹生成 |
| `HeroSystem` | 预留：技能 Tick、护盾衰减等全量扫描逻辑 |

### 5.4 AIEntity — 敌人实体

```csharp
public enum AIState { Idle = 0, Pursue = 1, Attack = 2, Dead = 3 }

public sealed class AIEntity
{
    public string Id { get; }
    public Vector3 Position { get; set; }
    public float MaxHp { get; }
    public float CurrentHp { get; }
    public bool IsAlive => CurrentHp > 0f && State != AIState.Dead;
    public AIState State { get; set; }
    public float VisionRadius { get; set; }
    public float AttackRange { get; set; }
    public Vector3 KnockbackVelocity { get; set; }
    public float StunRemaining { get; set; }
}
```

### 5.5 BulletEntity — 子弹 (Strategy 模式)

子弹使用 **Strategy 模式**实现不同的飞行轨迹：

```csharp
public sealed class BulletEntity
{
    public string OwnerId { get; }       // 发射者 (英雄)
    public string TargetId { get; }      // 目标 (敌人)
    public Vector3 Position { get; set; }
    public float Speed { get; set; }
    public float Damage { get; set; }
    public float HitRadius { get; set; }
    public IBulletMovement Movement { get; }  // ← 飞行策略

    // 策略实现: LinearMovement, ParabolicMovement, TrackingMovement
}
```

`BulletEntity.Tick()` 委托给 `Movement.UpdatePosition()`，新增弹道类型只需实现 `IBulletMovement` 即可。

### 5.6 EntityRegistry — 实体注册表

```csharp
public sealed class EntityRegistry
{
    public List<HeroEntity> Heroes { get; }    // 英雄列表
    public List<AIEntity> Enemies { get; }     // 敌人列表
    public List<BulletEntity> Bullets { get; } // 子弹列表

    public HeroEntity? FindHero(string id);
    public AIEntity? FindAI(string id);
    public HeroEntity? FirstAliveHero();
}
```

---

## 6. 命令系统

命令系统是**输入 → 逻辑**的桥梁，也是回放系统的基石。

### 6.1 核心接口

```csharp
public interface IFrameCommand
{
    CommandType Type { get; }      // Move / UseSkill / SelectTrait
    string SourceId { get; }      // 发出命令的实体
    int FrameIndex { get; }       // 命令发生的逻辑帧
}
```

### 6.2 已实现的命令

| 命令 | 用途 |
|------|------|
| `MoveCommand` | 英雄移动输入 |
| `UseSkillCommand` | 施放技能 |
| `SelectTraitCommand` | 选择词条 (三选一) |

### 6.3 OrderBus — 命令总线

```csharp
public sealed class OrderBus
{
    // 推送本地命令 → 写入缓冲 + 尝试网络发送
    void PushLocalCommand(IFrameCommand cmd);

    // 推送远端命令 → 仅写入缓冲 (多人模式)
    void PushRemoteCommand(IFrameCommand cmd);

    // 从网络拉取远端命令 (在固定 Tick 早期调用)
    void PullRemoteCommands(int frame);

    // 消费指定帧所有命令 → 调试追踪
    IReadOnlyList<IFrameCommand> DequeueFrameCommands(int frame);
}
```

### 6.4 命令流

```
玩家输入 / AI 决策
    ↓
FrameCommandBuffer.Add(cmd)       ← 收集一帧内所有命令
    ↓
[可选] NetAdapter.Send(cmd)       ← 网络发送
    ↓
OrderBus.DequeueFrameCommands()   ← 帧末统一消费
    ↓
[可选] 写入 ReplayRecord          ← 录制
```

---

## 7. 伤害管线

英雄和怪物共用一套伤害管线，通过**可组合的修正链**实现灵活扩展。

### 7.1 五步管线

```
┌──────────────────────────────────────────────────┐
│ 1. 构造 DamageContext                              │
│    → BaseDamage = 传入伤害值, 初始化 FinalDamage    │
├──────────────────────────────────────────────────┤
│ 2. 无敌帧检查 (HitReactionService)                 │
│    → IsInvincible(targetID) → 取消伤害            │
├──────────────────────────────────────────────────┤
│ 3. Buff 修正 (BuffSystem)                         │
│    → 攻击方增伤 Buff、受击方减伤 Buff              │
├──────────────────────────────────────────────────┤
│ 4. 词条修正 (TraitSystem)                         │
│    → 攻击方/受击方词条的 ModifyDamage() 钩子       │
├──────────────────────────────────────────────────┤
│ 5. 扣血 + 死亡事件                                 │
│    → 目标 HP -= FinalDamage                       │
│    → HP ≤ 0 → DeathService.OnEntityDeath()        │
└──────────────────────────────────────────────────┘
```

### 7.2 DamageContext

```csharp
public sealed class DamageContext
{
    public string AttackerId { get; }    // 攻击方 ID
    public string TargetId { get; }      // 受击方 ID
    public float BaseDamage { get; }     // 原始伤害 (不可变)
    public float FinalDamage { get; set; } // 最终伤害 (由 Buff/Trait 修改)
    public bool IsCancelled { get; set; }  // 是否被取消 (无敌/Buff免疫)
}
```

### 7.3 受击反应 (HitReactionService)

与伤害计算**完全解耦**，命中后独立处理：

| 反应类型 | 作用对象 | 效果 |
|----------|---------|------|
| **击退** | Hero + AI | 施加 KnockbackVelocity, 每帧衰减 (系数 8f) |
| **硬直** | Hero + AI | 设置 StunRemaining, 期间不能移动/攻击 |
| **无敌帧** | 仅 Hero | 设置 InvincibleRemaining, 期间免疫伤害 |

---

## 8. Buff 与词条系统

### 8.1 Buff 系统

```csharp
public interface IBuff
{
    string BuffId { get; }
    string OwnerId { get; }
    float Duration { get; }          // 持续时间
    float RemainingDuration { get; }

    void OnApply(BattleContext context);
    void OnRemove(BattleContext context);
    void ModifyDamage(DamageContext damageCtx);  // 伤害修正钩子
    void Tick(BattleContext context, float deltaTime);
}

// 已实现的 Hotfix Buff:
// - AttackSpeedBuff: 增加攻击速度
// - DamageBoostBuff: 增加伤害倍率
```

### 8.2 词条 (Trait) 系统

```csharp
public interface ITrait
{
    string TraitId { get; }
    string OwnerId { get; }

    void OnEquip(BattleContext context);     // 装备时触发
    void OnUnequip(BattleContext context);   // 卸载时触发
    void ModifyDamage(DamageContext damageCtx);  // 伤害修正钩子 (默认空)
}
```

### 8.3 词条 Offer 机制 (三选一)

```
ITraitOfferService.GenerateOffer(context, count=3)
  → 从 TraitPool 加权抽取
    → 排除已装备词条
      → BattleRng.NextWeighted() 确定性加权无放回抽取
        → 返回 N 个候选 TraitId
```

词条 Offer 服务使用**加权无放回抽样**，所有随机走 `BattleRng` 保证可复现。

### 8.4 Buff vs Trait 对比

| 维度 | Buff | Trait |
|------|------|-------|
| 生命周期 | 有时限 (Duration) | 永久 (整局有效) |
| 获取方式 | 战斗中触发 | 关卡间选择 (Roguelike) |
| 管理方式 | BuffRegistry + BuffSystem | TraitRegistry + TraitSystem |
| 数量限制 | 可叠加 | 受词条槽位限制 |

---

## 9. 波次与刷怪系统

### 9.1 WaveSystem 工作流程

```
当前波次活跃
  → WaveSystem.Tick 检测清场
    → 延迟倒计时 (DelayTimer)
      → SpawnSystem.Spawn() 刷新一波
        → 循环至 PendingWaveCount = 0
```

```csharp
public sealed class WaveSystem
{
    public int PendingWaveCount { get; }    // 剩余波次数 (供 VictoryRule 查询)

    public void Tick(BattleContext context, float deltaTime)
    {
        // 1. 检测当前波次是否清场
        // 2. 清场后递减延迟计时器
        // 3. 延迟到期 → 调用 SpawnSystem.Spawn() 触发下一波
    }
}
```

### 9.2 SpawnSystem

负责根据 `WaveConfig` 决定**刷什么、刷多少、刷哪里**。`WaveConfig` 可由策划通过 Excel 配置表控制每波的敌人类型、数量、刷新位置。

---

## 10. AI 系统

### 10.1 四状态机

```
        ┌──────┐
        │ Idle │ ← 无目标
        └──┬───┘
    发现目标 │
           ↓
        ┌────────┐
        │ Pursue │ ← 向目标移动
        └───┬────┘
    到达攻击距离│
              ↓
        ┌────────┐
        │ Attack │ ← 攻击目标
        └───┬────┘
       HP=0  │
            ↓
        ┌──────┐
        │ Dead │ ← 参与回收流程
        └──────┘
```

### 10.2 AISystem

```csharp
public sealed class AISystem
{
    public void Tick(BattleContext ctx, float dt)
    {
        foreach (AIEntity ai in ctx.Registry.Enemies)
        {
            ai.Behavior?.Update(ctx, ai, dt);  // 委托给 AIBehavior
        }
    }
}
```

### 10.3 设计要点

- `AIBehavior` 是一个可替换的行为策略对象，Hotfix 层可以注册新的行为类型
- 受击反应与 AI 行为解耦——硬直期间 `StunRemaining > 0` 时跳过 AI 状态机
- 与 `HeroTargetingService` 对称：敌人也需要寻敌 + 移动 + 攻击

---

## 11. 规则层: 玩法模式与胜负

### 11.1 三位一体

```
┌─────────────┐  ┌──────────────────┐  ┌─────────────┐
│  IPlayMode  │  │  IStageHandler   │  │ IVictoryRule │
│  玩法模式   │  │  关卡处理器      │  │  胜负规则    │
├─────────────┤  ├──────────────────┤  ├─────────────┤
│ 控制生命周期│  │ 控制波次/关卡流程│  │ 判断结束条件 │
│ 钩子        │  │                  │  │ 判定胜负     │
└─────────────┘  └──────────────────┘  └─────────────┘
```

### 11.2 接口定义

```csharp
public interface IPlayMode
{
    void OnBattleStart(BattleContext context);
    void OnBattleEnd(BattleContext context, bool isVictory);
}

public interface IStageHandler
{
    bool IsStageCleared { get; }
    void Tick(BattleContext context, float deltaTime);
}

public interface IVictoryRule
{
    bool IsBattleFinished { get; }
    bool IsVictory { get; }
    void Tick(BattleContext context, float deltaTime);
}
```

### 11.3 已实现的规则

| 类型 | 实现 | 说明 |
|------|------|------|
| PlayMode | `DefaultPlayMode` | 空实现, 保证生命周期钩子可运行 |
| PlayMode | `ElitePlayMode` (Hotfix) | 精英模式, 热更注册 |
| StageHandler | `DefaultStageHandler` | 单波次关卡 |
| StageHandler | `MultiWaveStageHandler` | 多波次关卡 |
| VictoryRule | `KillAllVictoryRule` | 全歼敌人胜利, 英雄阵亡战败 |
| VictoryRule | `TimeLimitVictoryRule` | 限时存活 |

### 11.4 KillAllVictoryRule 判定逻辑

```csharp
public void Tick(BattleContext context, float deltaTime)
{
    // 战败检测 (优先): 任意英雄 HP 归零
    for (var hero in context.Registry.Heroes)
        if (!hero.IsAlive) → Defeat

    // 胜利检测: 关卡流程已清场
    if (context.StageHandler.IsStageCleared) → Victory
}
```

---

## 12. 回放系统

### 12.1 确定性保证

回放的可靠性依赖于三大支柱：

```
RngSeed (确定性种子) + 固定帧 30FPS + 命令帧录制 = 完全可复现
```

所有战斗内随机都必须且只能通过 `BattleRng` 产生，严禁使用 `UnityEngine.Random` 或 `System.Random`。

### 12.2 录制与回放流程

```
【录制】
  战斗每帧:
    → 收集本帧所有 IFrameCommand
    → 写入 ReplayFrameData (帧号 + 命令列表)
    → 战斗结束时导出 ReplayRecord (含 Loadout + 全部帧数据)

【回放】
  BattleBootstrap.EnterReplay(ReplayRecord)
    → 创建 BattleReplaySession
    → FrameLoop.Start()
    → 每逻辑帧:
        → ReplayNetAdapter 从 ReplayRecord 读取对应帧的命令
        → 不再依赖玩家输入, 完全按录制帧序列消费命令
```

### 12.3 相关文件

| 文件 | 职责 |
|------|------|
| `IReplayService.cs` | 回放服务接口 |
| `LocalReplayService.cs` | 本地录制实现 |
| `BattleReplaySession.cs` | 回放会话管理 |
| `ReplayRecord.cs` | 录像数据结构 (Loadout + 帧数据) |
| `ReplayFrameData.cs` | 单帧数据 |
| `ReplayNetAdapter.cs` | 回放时模拟的网络适配器 |
| `MissFrameRequest.cs` | 补帧请求 (预留网络同步) |

---

## 13. 配置系统

### 13.1 设计理念

```
Excel (策划编写) → 类型行检测 → 校验 (主键/外键/类型) → 生成 C# 类 + Asset → 运行时类型安全查询
```

### 13.2 配置表约定

| 约定 | 说明 |
|------|------|
| **文件格式** | Excel (.xlsx) 优先, 兼容 CSV (UTF-8) |
| **Sheet 名 = 逻辑表名** | 如 Sheet 叫 `Attr`, 则生成 `Attr` 表 |
| **第一行: 列名** | 稳定英文标识 |
| **第二行 (可选): 类型行** | 声明每列类型 (int / float / string / vector3 / ...) |
| **第三行起: 数据** | 主键列统一使用 `Id`, 不能为空/重复 |

### 13.3 类型行支持的 token

| 类型 token | C# 类型 | 格式 |
|------------|---------|------|
| `int` | `int` | 十进制整数 |
| `long` | `long` | 十进制整数 |
| `float` | `float` | 浮点 |
| `double` | `double` | 浮点 |
| `string` | `string` | 任意文本 |
| `vector2` | `Vector2` | `x,y` |
| `vector3` | `Vector3` | `x,y,z` |
| `color` | `Color` | `r,g,b,a` 或 `#RRGGBB` |
| `int[]` | `int[]` | 逗号分隔 |

### 13.4 外键校验

```csharp
// Assets/Game/Config/Editor/Validation/ConfigReferenceRules.cs
new ReferenceRule("Buff", "AttrId", "Attr", "Id")
// → Buff 表的 AttrId 列的值必须存在于 Attr 表的 Id 列
```

### 13.5 生成流程

1. 策划更新 `Editor/Excel/Samples/*.xlsx`
2. Unity 菜单: `Tools/Config/Generation Window`
3. 点击 `Refresh Table List From Samples` → 同步 manifest
4. 勾选 `Enabled` → `Generate Enabled`
5. 输出:
   - `Tables/*.gen.cs` (自动生成的强类型 C# 配置类)
   - `Data/*ConfigRaw.asset` (运行时加载的配置资产)
   - `generation-summary.json` (CI 用)

### 13.6 运行时读取

```csharp
public interface IConfigProvider
{
    T Get<T>(string key);
}
// 战斗核心不直接依赖 Excel/ScriptableObject/Addressables
// 由外部注入实现 (依赖反转)
```

---

## 14. Hotfix 热更层

### 14.1 注入机制

Hotfix 层通过 `setupContext` 回调注入到 Runtime 层：

```csharp
// BattleBootstrap.EnterBattle
bootstrap.EnterBattle(
    loadout: myLoadout,
    setupContext: ctx => {
        // Hotfix 层在此注册所有自定义内容
        HotfixDefRegistrar.Register(ctx);      // HeroDef, WeaponDef
        HotfixTraitRegistrar.Register(ctx);    // TraitFactory, TraitPool
    }
);
```

Runtime 层通过 `setupContext` 机制**完全不感知 Hotfix 层的存在**——这正是框架设计的关键解耦点。

### 14.2 可热更的内容

| 目录 | 内容 | 热更后 |
|------|------|--------|
| `Hotfix/Buffs/` | 自定义 Buff 类型 | 新 Buff 即时可用 |
| `Hotfix/Traits/` | 自定义词条 | 新词条即时可用 |
| `Hotfix/PlayModes/` | 新玩法模式 | 新玩法即时可用 |
| `Hotfix/BattleRules/` | 战斗规则 | 自定义规则即时可用 |
| `Hotfix/Bootstrap/` | 注册入口 | 变更注册逻辑即时生效 |

### 14.3 已实现的 Hotfix 内容

```csharp
// Buffs
public class AttackSpeedBuff : IBuff { ... }
public class DamageBoostBuff : IBuff { ... }

// Traits
public class DamageBoostTrait : ITrait { ... }

// PlayModes
public class ElitePlayMode : IPlayMode { ... }

// 注册入口
public static class HotfixAllRegistrar
{
    public static void RegisterAll(BattleContext ctx)
    {
        HotfixDefRegistrar.Register(ctx);
        HotfixTraitRegistrar.Register(ctx);
    }
}
```

---

## 15. 扩展指南

### 15.1 新增 Buff

1. 在 `Hotfix/Buffs/` 下创建 `MyNewBuff.cs`
2. 实现 `IBuff` 接口
3. 在 `HotfixAllRegistrar` 或自定义注册器中注册

```csharp
public class MyNewBuff : IBuff
{
    public void OnApply(BattleContext ctx) { /* 效果生效 */ }
    public void OnRemove(BattleContext ctx) { /* 效果移除 */ }
    public void ModifyDamage(DamageContext ctx) { /* 伤害修正 */ }
}
```

### 15.2 新增词条

1. 在 `Hotfix/Traits/` 下创建 `MyNewTrait.cs`
2. 实现 `ITrait` 接口
3. 在 `HotfixTraitRegistrar` 中注册到 `TraitFactory`

### 15.3 新增玩法模式

1. 实现 `IPlayMode` 接口
2. 在 `Hotfix/PlayModes/` 中创建 `MyPlayMode.cs`
3. 通过 `setupContext` 注入到 `BattleContext`

### 15.4 新增子弹类型

1. 实现 `IBulletMovement` 接口
2. 在 `BulletFactory` 中注册新类型
3. 运行时根据 WeaponDef 自动路由

### 15.5 新增命令

1. 实现 `IFrameCommand` 接口
2. 在 `BattleWorld` 的固定帧消费逻辑中添加对应处理
3. 回放系统自动支持 (因为命令帧统一录制)

---

## 附录: 文件清单

```
BattleNew/Assets/Game/Battle/
├── Runtime/
│   ├── Bootstrap/          (2 files)  BattleBootstrap, BattleRunnerBehaviour
│   ├── Commands/           (6 files)  IFrameCommand, OrderBus, MoveCommand...
│   ├── Core/               (6 files)  BattleWorld, BattleContext, FrameLoop...
│   ├── Entities/
│   │   ├── AI/             (2 files)  AIEntity, AISystem
│   │   ├── Buff/           (4 files)  IBuff, BuffRuntime, BuffSystem, BuffRegistry
│   │   ├── Bullet/         (12 files) BulletEntity, WeaponFireService, HitResolver...
│   │   ├── Element/        (5 files)  DamageService, HitReactionService, DeathService
│   │   ├── Hero/           (8 files)  HeroEntity, HeroMovementController...
│   │   ├── Trait/          (8 files)  ITrait, TraitSystem, TraitOfferService...
│   │   └── Wave/           (3 files)  WaveSystem, SpawnSystem, WaveConfig
│   ├── Rules/
│   │   ├── PlayModes/      (2 files)  IPlayMode, DefaultPlayMode
│   │   ├── StageHandlers/  (3 files)  IStageHandler, Default/MultiWave StageHandler
│   │   └── VictoryRules/   (3 files)  IVictoryRule, KillAllVictoryRule, TimeLimit
│   ├── Services/
│   │   ├── Config/         (1 file)   IConfigProvider
│   │   ├── DebugTrace/     (3 files)  IDebugTraceService, Null/Unity 实现
│   │   ├── Events/         (2 files)  IEventBus, InMemoryEventBus
│   │   ├── Network/        (2 files)  INetAdapter, NullNetAdapter
│   │   └── Replay/         (6 files)  IReplayService, BattleReplaySession...
│   └── Presentation/       (1 file)   BattleSimpleVisualizer (HUD)
└── Hotfix/
    ├── Bootstrap/          (4 files)  HotfixAllRegistrar, HotfixDefRegistrar...
    ├── Buffs/              (2 files)  AttackSpeedBuff, DamageBoostBuff
    ├── Traits/             (1 file)   DamageBoostTrait
    ├── PlayModes/          (1 file)   ElitePlayMode
    └── BattleRules/        (README)
```

---

> **总结**: BattleNew 是一个接口驱动、分层清晰、支持热更的固定帧战斗框架。它的设计哲学是"运行时零感知、配置全驱动、回放可复现"。适合作为 Roguelike、塔防、弹幕射击等 2D/3D 战斗游戏的核心引擎。
