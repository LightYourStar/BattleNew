using Game.Battle.Runtime.Commands;
using Game.Battle.Runtime.Commands.Commands;
using Game.Battle.Runtime.Entities.Hero;
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
    /// Battle runtime orchestrator; keeps update order and module boundaries explicit.
    /// </summary>
    public sealed class BattleWorld
    {
        public BattleContext Context { get; }

        public FrameLoop FrameLoop { get; }

        public BattleWorld(
            float fixedStep = 1f / 30f,
            IReplayService? replayService = null,
            INetAdapter? netAdapter = null,
            IEventBus? eventBus = null,
            IDebugTraceService? debugTraceService = null,
            IConfigProvider? configProvider = null)
        {
            FrameLoop = new FrameLoop(fixedStep);
            Context = BuildContext(
                fixedStep,
                replayService ?? new LocalReplayService(),
                netAdapter ?? new NullNetAdapter(),
                eventBus ?? new InMemoryEventBus(),
                debugTraceService ?? new NullDebugTraceService(),
                configProvider);

            FrameLoop.OnFixedTick += Tick;
        }

        public void Start()
        {
            Context.PlayMode.OnBattleStart(Context);
            FrameLoop.Start();
        }

        public void Pause()
        {
            FrameLoop.Pause();
        }

        public void Resume()
        {
            FrameLoop.Resume();
        }

        public void Stop()
        {
            FrameLoop.Stop();
            Context.PlayMode.OnBattleEnd(Context, Context.VictoryRule.IsVictory);
        }

        public void Update(float deltaTime)
        {
            FrameLoop.Tick(deltaTime);
        }

        private void Tick(float deltaTime)
        {
            int frame = Context.Time.Frame;
            Context.DebugTraceService.TraceFrameAdvance(frame);

            Context.OrderBus.PullRemoteCommands(frame);
            var frameCommands = Context.OrderBus.DequeueFrameCommands(frame);
            Context.ReplayService.RecordFrameCommands(frame, frameCommands);
            ConsumeCommands(frameCommands, deltaTime);

            // Fixed order contract from migration docs:
            // 1) Commands, then 2-9 system/rule layers.
            Context.HeroSystem.Tick(Context, deltaTime);
            Context.AISystem.Tick(Context, deltaTime);
            Context.BulletSystem.Tick(Context, deltaTime);
            Context.BuffSystem.Tick(Context, deltaTime);
            Context.TraitSystem.Tick(Context, deltaTime);
            Context.WaveSystem.Tick(Context, deltaTime);
            Context.StageHandler.Tick(Context, deltaTime);
            Context.VictoryRule.Tick(Context, deltaTime);

            Context.Time.AdvanceOneFrame();

            if (Context.VictoryRule.IsBattleFinished)
            {
                Stop();
            }
        }

        private void ConsumeCommands(System.Collections.Generic.IReadOnlyList<IFrameCommand> frameCommands, float deltaTime)
        {
            for (int i = 0; i < frameCommands.Count; i++)
            {
                IFrameCommand command = frameCommands[i];
                switch (command)
                {
                    case MoveCommand moveCommand:
                        ApplyMove(moveCommand, deltaTime);
                        break;
                    case UseSkillCommand:
                    case SelectTraitCommand:
                        // Reserved for later slices.
                        break;
                }
            }
        }

        private void ApplyMove(MoveCommand moveCommand, float deltaTime)
        {
            for (int i = 0; i < Context.Registry.Heroes.Count; i++)
            {
                HeroEntity hero = Context.Registry.Heroes[i];
                if (hero.Id != moveCommand.HeroId)
                {
                    continue;
                }

                Context.HeroSystem.ApplyMove(hero, moveCommand.Direction, deltaTime);
                return;
            }
        }

        private static BattleContext BuildContext(
            float fixedStep,
            IReplayService replayService,
            INetAdapter netAdapter,
            IEventBus eventBus,
            IDebugTraceService debugTraceService,
            IConfigProvider? configProvider)
        {
            BattleContext context = new(
                time: new BattleTime(fixedStep),
                registry: new Entities.EntityRegistry(),
                replayService: replayService,
                netAdapter: netAdapter,
                eventBus: eventBus,
                debugTraceService: debugTraceService,
                configProvider: configProvider);

            FrameCommandBuffer commandBuffer = new();
            context.OrderBus = new OrderBus(commandBuffer, netAdapter, debugTraceService);
            context.HeroSystem = new Entities.Hero.HeroSystem();
            context.AISystem = new Entities.AI.AISystem();
            context.BulletSystem = new Entities.Bullet.BulletSystem();
            context.BuffSystem = new Entities.Buff.BuffSystem();
            context.TraitSystem = new Entities.Trait.TraitSystem();
            context.WaveSystem = new Entities.Wave.WaveSystem();
            context.PlayMode = new DefaultPlayMode();
            context.StageHandler = new DefaultStageHandler();
            context.VictoryRule = new KillAllVictoryRule();

            return context;
        }
    }
}
