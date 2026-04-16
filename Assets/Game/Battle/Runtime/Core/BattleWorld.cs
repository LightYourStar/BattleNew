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
    /// 战斗世界：负责装配 <see cref="BattleContext"/>，并驱动固定帧循环下的系统更新顺序。
    /// <para>
    /// 边界原则：
    /// - BattleWorld 是“编排器”，不要把具体玩法规则写进来（规则在 Rules 层）。
    /// - 高频逻辑在固定 Tick 内完成；渲染表现应监听数据变化或由 Presentation 同步。
    /// </para>
    /// </summary>
    public sealed class BattleWorld
    {
        /// <summary>对外暴露的运行时上下文（系统、服务、实体表）。</summary>
        public BattleContext Context { get; }

        /// <summary>固定步长循环（与 Unity Update 解耦）。</summary>
        public FrameLoop FrameLoop { get; }

        /// <summary>
        /// 创建战斗世界并装配默认依赖（可用参数替换为自定义实现以便测试）。
        /// </summary>
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

        /// <summary>开始战斗：触发玩法开始钩子并启动固定循环。</summary>
        public void Start()
        {
            Context.PlayMode.OnBattleStart(Context);
            FrameLoop.Start();
        }

        /// <summary>暂停战斗逻辑（渲染可不暂停）。</summary>
        public void Pause()
        {
            FrameLoop.Pause();
        }

        /// <summary>恢复战斗逻辑。</summary>
        public void Resume()
        {
            FrameLoop.Resume();
        }

        /// <summary>
        /// 结束战斗：停止循环并触发玩法结束钩子。
        /// <para>
        /// 注意：当前实现会停止 FrameLoop，但不会自动解除 OnFixedTick 订阅；
        /// 若未来需要频繁创建/销毁 BattleWorld，可补充 IDisposable 来解绑事件，避免重复订阅。
        /// </para>
        /// </summary>
        public void Stop()
        {
            FrameLoop.Stop();
            Context.PlayMode.OnBattleEnd(Context, Context.VictoryRule.IsVictory);
        }

        /// <summary>由外部驱动（通常是 MonoBehaviour.Update）调用，用渲染 deltaTime 推进逻辑帧。</summary>
        public void Update(float deltaTime)
        {
            FrameLoop.Tick(deltaTime);
        }

        /// <summary>
        /// 单个固定逻辑帧推进：命令 -> 各系统 -> 关卡 -> 胜负。
        /// </summary>
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

        /// <summary>消费本帧命令：当前实现仅处理移动，其余命令类型在后续切片接入。</summary>
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

        /// <summary>将移动命令应用到指定英雄实体。</summary>
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

        /// <summary>装配默认 Context：创建依赖与系统实例。</summary>
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
