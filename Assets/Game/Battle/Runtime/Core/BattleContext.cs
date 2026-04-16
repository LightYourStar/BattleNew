using Game.Battle.Runtime.Commands;
using Game.Battle.Runtime.Entities;
using Game.Battle.Runtime.Entities.AI;
using Game.Battle.Runtime.Entities.Buff;
using Game.Battle.Runtime.Entities.Bullet;
using Game.Battle.Runtime.Entities.Hero;
using Game.Battle.Runtime.Entities.Trait;
using Game.Battle.Runtime.Entities.Wave;
using Game.Battle.Runtime.Rules.PlayModes;
using Game.Battle.Runtime.Rules.StageHandlers;
using Game.Battle.Runtime.Rules.VictoryRules;
using Game.Battle.Runtime.Services.Config;
using Game.Battle.Runtime.Services.DebugTrace;
using Game.Battle.Runtime.Services.Events;
using Game.Battle.Runtime.Services.Network;
using Game.Battle.Runtime.Services.Replay;

namespace Game.Battle.Runtime.Core
{
    /// <summary>
    /// 战斗运行时上下文：聚合“时间轴、实体注册表、跨系统服务、各系统实例”。
    /// <para>
    /// 设计说明：
    /// - Context 是依赖注入的承载体，避免系统之间互相 new 来 new 去。
    /// - 系统字段由 <see cref="BattleWorld"/> 负责装配，保持构造函数简单稳定。
    /// </para>
    /// </summary>
    public sealed class BattleContext
    {
        /// <summary>逻辑时间轴（帧号 + 固定步长）。</summary>
        public BattleTime Time { get; }

        /// <summary>实体注册表：英雄/敌人/子弹等运行时实体集合。</summary>
        public EntityRegistry Registry { get; }

        /// <summary>回放服务：记录/读取命令帧（占位到切片 10 逐步增强）。</summary>
        public IReplayService ReplayService { get; }

        /// <summary>网络适配器：发送/拉取远端命令（单机默认空实现）。</summary>
        public INetAdapter NetAdapter { get; }

        /// <summary>事件总线：用于非确定性通知（与命令系统解耦）。</summary>
        public IEventBus EventBus { get; }

        /// <summary>可选配置提供者：核心逻辑通过接口读取配置，避免绑定具体配表方案。</summary>
        public IConfigProvider? ConfigProvider { get; }

        /// <summary>调试追踪：日志/采样/Editor 可视化入口。</summary>
        public IDebugTraceService DebugTraceService { get; }

        /// <summary>命令总线：写入/拉取/消费命令的统一入口。</summary>
        public OrderBus OrderBus { get; internal set; } = default!;

        /// <summary>英雄系统：移动/攻击节奏/索敌等（当前为最小闭环实现）。</summary>
        public HeroSystem HeroSystem { get; internal set; } = default!;

        /// <summary>AI 系统：敌人行为与状态切换（当前为最小闭环实现）。</summary>
        public AISystem AISystem { get; internal set; } = default!;

        /// <summary>子弹系统：飞行、命中、伤害触发（当前为最小闭环实现）。</summary>
        public BulletSystem BulletSystem { get; internal set; } = default!;

        /// <summary>Buff 系统：后续切片实现（当前占位）。</summary>
        public BuffSystem BuffSystem { get; internal set; } = default!;

        /// <summary>词条系统：后续切片实现（当前占位）。</summary>
        public TraitSystem TraitSystem { get; internal set; } = default!;

        /// <summary>波次系统：刷怪/波次推进（当前为最小示例）。</summary>
        public WaveSystem WaveSystem { get; internal set; } = default!;

        /// <summary>玩法模式：玩法层生命周期钩子（开始/结束）。</summary>
        public IPlayMode PlayMode { get; internal set; } = default!;

        /// <summary>关卡流程：波次/阶段条件等（当前为默认实现）。</summary>
        public IStageHandler StageHandler { get; internal set; } = default!;

        /// <summary>胜负规则：胜利/失败判定与战斗结束标记。</summary>
        public IVictoryRule VictoryRule { get; internal set; } = default!;

        /// <summary>
        /// 创建上下文（系统字段随后由 <see cref="BattleWorld"/> 装配完成）。
        /// </summary>
        public BattleContext(
            BattleTime time,
            EntityRegistry registry,
            IReplayService replayService,
            INetAdapter netAdapter,
            IEventBus eventBus,
            IDebugTraceService debugTraceService,
            IConfigProvider? configProvider = null)
        {
            Time = time;
            Registry = registry;
            ReplayService = replayService;
            NetAdapter = netAdapter;
            EventBus = eventBus;
            DebugTraceService = debugTraceService;
            ConfigProvider = configProvider;
        }
    }
}
