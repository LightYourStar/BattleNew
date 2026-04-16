using System.Collections.Generic;
using Game.Battle.Runtime.Commands;
using Game.Battle.Runtime.Commands.Commands;
using Game.Battle.Runtime.Entities;
using Game.Battle.Runtime.Entities.AI;
using Game.Battle.Runtime.Entities.Bullet;
using Game.Battle.Runtime.Entities.Element;
using Game.Battle.Runtime.Entities.Hero;
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
    /// 战斗世界：负责装配 BattleContext 并驱动固定帧循环下的系统更新顺序。
    /// <para>
    /// BattleWorld 是纯粹的"编排者"，不包含任何业务逻辑：
    /// 每帧只负责按顺序调用各系统的 Tick，具体逻辑由各系统/服务自行处理。
    /// </para>
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

        public void Pause() => FrameLoop.Pause();
        public void Resume() => FrameLoop.Resume();

        public void Stop()
        {
            FrameLoop.Stop();
            Context.PlayMode.OnBattleEnd(Context, Context.VictoryRule.IsVictory);
        }

        public void Update(float deltaTime)
        {
            FrameLoop.Tick(deltaTime);
        }

        /// <summary>
        /// 固定逻辑帧推进，严格按文档约定顺序：
        /// 命令消费 → Hero → AI → Bullet → Buff → Trait → Wave → Stage → Victory
        /// </summary>
        private void Tick(float deltaTime)
        {
            int frame = Context.Time.Frame;
            Context.DebugTraceService.TraceFrameAdvance(frame);

            // ① 命令收集与消费
            Context.OrderBus.PullRemoteCommands(frame);
            IReadOnlyList<IFrameCommand> frameCommands = Context.OrderBus.DequeueFrameCommands(frame);
            Context.ReplayService.RecordFrameCommands(frame, frameCommands);
            ConsumeCommands(frameCommands, deltaTime);

            // ② Hero：状态更新 + 武器发射
            for (int i = 0; i < Context.Registry.Heroes.Count; i++)
            {
                HeroEntity hero = Context.Registry.Heroes[i];
                string? targetId = Context.HeroTargetingService.FindNearestAliveEnemy(Context, hero);
                hero.LockedTargetId = targetId;
                Context.HeroStateController.UpdateState(Context, hero, targetId != null, hero.IsMovingThisFrame);
                Context.WeaponFireService.TryFire(Context, hero, targetId, deltaTime);
                hero.IsMovingThisFrame = false;
            }

            // ③ AI：状态 + 追击 + 攻击
            Context.AISystem.Tick(Context, deltaTime);

            // ④ Bullet：委托给 BulletSystem（飞行策略由各子弹自己持有，命中由 HitResolver 处理）
            Context.BulletSystem.Tick(Context, deltaTime);

            // ⑤ Buff
            Context.BuffSystem.Tick(Context, deltaTime);

            // ⑥ Trait（预留每帧效果）
            Context.TraitSystem.Tick(Context, deltaTime);

            // ⑦ Wave
            Context.WaveSystem.Tick(Context, deltaTime);

            // ⑧ Stage
            Context.StageHandler.Tick(Context, deltaTime);

            // ⑨ Victory
            Context.VictoryRule.Tick(Context, deltaTime);

            Context.Time.AdvanceOneFrame();

            if (Context.VictoryRule.IsBattleFinished)
            {
                Stop();
            }
        }

        private void ConsumeCommands(IReadOnlyList<IFrameCommand> frameCommands, float deltaTime)
        {
            for (int i = 0; i < frameCommands.Count; i++)
            {
                IFrameCommand command = frameCommands[i];
                switch (command)
                {
                    case MoveCommand moveCommand:
                        ApplyMove(moveCommand, deltaTime);
                        break;
                    case UseSkillCommand skillCommand:
                        ApplySkill(skillCommand);
                        break;
                    case SelectTraitCommand traitCommand:
                        ApplySelectTrait(traitCommand);
                        break;
                }
            }
        }

        private void ApplyMove(MoveCommand cmd, float deltaTime)
        {
            for (int i = 0; i < Context.Registry.Heroes.Count; i++)
            {
                HeroEntity hero = Context.Registry.Heroes[i];
                if (hero.Id != cmd.HeroId)
                {
                    continue;
                }

                Context.HeroSystem.ApplyMove(hero, cmd.Direction, deltaTime);
                hero.IsMovingThisFrame = true;
                return;
            }
        }

        private void ApplySkill(UseSkillCommand cmd)
        {
            Context.DebugTraceService.TraceStateChange(cmd.CasterId, "Idle", $"Skill:{cmd.SkillId}");
            // TODO: 后续接入技能系统
        }

        private void ApplySelectTrait(SelectTraitCommand cmd)
        {
            Context.DebugTraceService.TraceStateChange(cmd.HeroId, "NoTrait", $"Trait:{cmd.TraitId}");
            // TODO: 后续接入词条选择系统
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
                registry: new EntityRegistry(),
                replayService: replayService,
                netAdapter: netAdapter,
                eventBus: eventBus,
                debugTraceService: debugTraceService,
                configProvider: configProvider);

            FrameCommandBuffer commandBuffer = new();
            context.OrderBus = new OrderBus(commandBuffer, netAdapter, debugTraceService);

            // 实体系统
            context.HeroSystem = new Entities.Hero.HeroSystem();
            context.HeroTargetingService = new HeroTargetingService();
            context.HeroStateController = new HeroStateController();
            context.AISystem = new Entities.AI.AISystem();
            context.BulletSystem = new Entities.Bullet.BulletSystem();
            context.BuffSystem = new Entities.Buff.BuffSystem();
            context.TraitSystem = new Entities.Trait.TraitSystem();
            context.WaveSystem = new Entities.Wave.WaveSystem();
            context.SpawnSystem = new SpawnSystem();

            // 子弹链
            context.BulletFactory = new BulletFactory();
            context.WeaponFireService = new WeaponFireService(context.BulletFactory);
            context.HitResolver = new HitResolver();

            // 伤害/死亡
            context.DamageService = new DamageService();
            context.DeathService = new DeathService();
            context.HitReactionService = new HitReactionService();

            // 规则
            context.PlayMode = new DefaultPlayMode();
            context.StageHandler = new DefaultStageHandler();
            // context.StageHandler = new MultiWaveStageHandler(10);

            context.VictoryRule = new KillAllVictoryRule();

            return context;
        }
    }
}
