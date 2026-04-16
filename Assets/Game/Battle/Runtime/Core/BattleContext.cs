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
    /// Runtime dependency hub for battle frame simulation.
    /// </summary>
    public sealed class BattleContext
    {
        public BattleTime Time { get; }

        public EntityRegistry Registry { get; }

        public IReplayService ReplayService { get; }

        public INetAdapter NetAdapter { get; }

        public IEventBus EventBus { get; }

        public IConfigProvider? ConfigProvider { get; }

        public IDebugTraceService DebugTraceService { get; }

        public OrderBus OrderBus { get; internal set; } = default!;

        public HeroSystem HeroSystem { get; internal set; } = default!;

        public AISystem AISystem { get; internal set; } = default!;

        public BulletSystem BulletSystem { get; internal set; } = default!;

        public BuffSystem BuffSystem { get; internal set; } = default!;

        public TraitSystem TraitSystem { get; internal set; } = default!;

        public WaveSystem WaveSystem { get; internal set; } = default!;

        public IPlayMode PlayMode { get; internal set; } = default!;

        public IStageHandler StageHandler { get; internal set; } = default!;

        public IVictoryRule VictoryRule { get; internal set; } = default!;

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
