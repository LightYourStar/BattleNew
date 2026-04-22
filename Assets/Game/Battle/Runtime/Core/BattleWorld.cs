using System;
using System.Collections.Generic;
using Game.Battle.Runtime.Commands;
using Game.Battle.Runtime.Commands.Commands;
using Game.Battle.Runtime.Entities;
using Game.Battle.Runtime.Entities.AI;
using Game.Battle.Runtime.Entities.Bullet;
using Game.Battle.Runtime.Entities.Element;
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
using UnityEngine;

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

        // ─── 播种 ──────────────────────────────────────────────────────────────

        /// <summary>
        /// 根据 <see cref="BattleLoadout"/> 播种初始实体并初始化 RNG / 词条 Offer 服务。
        /// <para>
        /// 流程：
        /// <list type="number">
        ///   <item>写入 <see cref="BattleContext.Loadout"/>，按 RngSeed 创建 <see cref="BattleRng"/>。</item>
        ///   <item>查找 <see cref="HeroDef"/>，用其基础属性创建 <see cref="HeroEntity"/>。</item>
        ///   <item>查找 <see cref="WeaponDef"/>（先用 Loadout.WeaponDefId，再回退 HeroDef.DefaultWeaponId），
        ///         创建 <see cref="WeaponRuntime"/> 挂到英雄。</item>
        ///   <item>合并英雄 + 武器词条池，创建 <see cref="TraitOfferService"/>。</item>
        /// </list>
        /// </para>
        /// </summary>
        public void SeedFromLoadout(BattleLoadout loadout)
        {
            Context.Loadout = loadout;

            // RNG：种子为 0 时自动用时间戳
            long seed = loadout.RngSeed != 0
                ? loadout.RngSeed
                : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Context.Rng = new BattleRng(seed);
            loadout.RngSeed = seed; // 写回（确保 ReplayRecord 携带实际使用的种子）

            // 查找英雄定义（可为 null，退化为默认英雄）
            HeroDef? heroDef = string.IsNullOrEmpty(loadout.HeroDefId)
                ? null
                : Context.HeroDefRegistry.Get(loadout.HeroDefId);

            // 创建英雄实体
            HeroEntity hero = new("hero_1", Vector3.zero)
            {
                DefId = heroDef?.Id ?? string.Empty,
                MaxHp = heroDef?.MaxHp ?? 100f,
                CurrentHp = heroDef?.MaxHp ?? 100f,
                MoveSpeed = heroDef?.MoveSpeed ?? 5f,
                AttackRange = heroDef?.AttackRange ?? 5f,
            };

            // 查找武器定义：优先 Loadout.WeaponDefId，其次 HeroDef.DefaultWeaponId
            string weaponId = !string.IsNullOrEmpty(loadout.WeaponDefId)
                ? loadout.WeaponDefId
                : heroDef?.DefaultWeaponId ?? string.Empty;

            WeaponDef? weaponDef = string.IsNullOrEmpty(weaponId)
                ? null
                : Context.WeaponDefRegistry.Get(weaponId);

            if (weaponDef != null)
            {
                hero.CurrentWeapon = new WeaponRuntime(weaponDef);
            }
            else
            {
                // 未注册武器时使用内置默认武器（保持最小闭环可运行）
                hero.CurrentWeapon = new WeaponRuntime(FallbackWeaponDef());
            }

            Context.Registry.Heroes.Add(hero);

            // 词条池：合并 Loadout.TraitPoolIds（由上层传入，已融合英雄池 + 武器池）
            string[] poolIds = loadout.TraitPoolIds.Length > 0
                ? loadout.TraitPoolIds
                : MergeTraitPoolIds(heroDef, weaponDef);

            List<TraitPoolEntry> resolvedPool = Context.TraitPoolRegistry.Resolve(poolIds);
            Context.TraitOfferService = new TraitOfferService(resolvedPool);
        }

        // ─── 固定帧 Tick ────────────────────────────────────────────────────────

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

            // ① ½ 受击反应 Tick（击退位移 + 计时器衰减；放在行为系统前，避免当帧重复叠加）
            Context.HitReactionService.Tick(Context, deltaTime);

            // ② Hero：锁敌 → 朝向 → 状态 → 武器发射 → 帧标记清零
            for (int i = 0; i < Context.Registry.Heroes.Count; i++)
            {
                HeroEntity hero = Context.Registry.Heroes[i];

                // 锁敌（HeroTargetingService）
                string? targetId = Context.HeroTargetingService.FindNearestAliveEnemy(Context, hero);
                hero.LockedTargetId = targetId;

                // 朝向：有目标且未在移动时，面朝目标
                if (!hero.IsMovingThisFrame && targetId != null)
                {
                    var target = Context.Registry.FindAI(targetId);
                    if (target != null)
                    {
                        Context.HeroMovementController.ApplyFaceTarget(hero, target.Position);
                    }
                }

                // 状态切换（HeroStateController）
                Context.HeroStateController.UpdateState(Context, hero, targetId != null, hero.IsMovingThisFrame);

                // 武器发射（WeaponFireService：硬直 / 冷却 / 枪口迭代 / 弹道路由）
                Context.WeaponFireService.TryFire(Context, hero, targetId, deltaTime);

                // 清帧标记
                hero.IsMovingThisFrame = false;
                hero.MoveDirectionThisFrame = Vector3.zero;
            }

            // ③ AI：状态 + 追击 + 攻击
            Context.AISystem.Tick(Context, deltaTime);

            // ④ Bullet：飞行策略 + 命中判定
            Context.BulletSystem.Tick(Context, deltaTime);

            // ⑤ Buff
            Context.BuffSystem.Tick(Context, deltaTime);

            // ⑥ Trait（每帧效果）
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
            HeroEntity? hero = Context.Registry.FindHero(cmd.HeroId);
            if (hero == null)
            {
                return;
            }

            // 硬直期间忽略移动命令
            if (hero.StunRemaining > 0f)
            {
                return;
            }

            Context.HeroMovementController.ApplyMove(hero, cmd.Direction, deltaTime);
            hero.IsMovingThisFrame = true;
            hero.MoveDirectionThisFrame = cmd.Direction;
        }

        private void ApplySkill(UseSkillCommand cmd)
        {
            Context.DebugTraceService.TraceStateChange(cmd.CasterId, "Idle", $"Skill:{cmd.SkillId}");
            // TODO: 后续接入技能系统
        }

        private void ApplySelectTrait(SelectTraitCommand cmd)
        {
            if (!Context.TraitFactory.TryCreate(cmd.TraitId, cmd.HeroId, out ITrait? trait) || trait == null)
            {
                Context.DebugTraceService.TraceStateChange(cmd.HeroId, "NoTrait", $"TraitNotFound:{cmd.TraitId}");
                return;
            }

            Context.TraitSystem.EquipTrait(Context, trait);
            Context.DebugTraceService.TraceStateChange(cmd.HeroId, "NoTrait", $"Trait:{cmd.TraitId}");
        }

        // ─── BuildContext ───────────────────────────────────────────────────────

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

            // 数据注册表（Hotfix/setupContext 回调填充内容）
            context.HeroDefRegistry = new HeroDefRegistry();
            context.WeaponDefRegistry = new WeaponDefRegistry();
            context.TraitPoolRegistry = new TraitPoolRegistry();

            // 实体系统
            context.HeroSystem = new Entities.Hero.HeroSystem();
            context.HeroMovementController = new HeroMovementController();
            context.HeroTargetingService = new HeroTargetingService();
            context.HeroStateController = new HeroStateController();
            context.AISystem = new Entities.AI.AISystem();
            context.BulletSystem = new Entities.Bullet.BulletSystem();
            context.BuffSystem = new Entities.Buff.BuffSystem();
            context.TraitSystem = new Entities.Trait.TraitSystem();
            context.TraitFactory = new Entities.Trait.TraitFactory();
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
            context.VictoryRule = new KillAllVictoryRule();

            // Rng / TraitOfferService / Loadout 由 SeedFromLoadout 在 BuildContext 之后写入；
            // 先赋予确定性默认值，防止 Tick 前的 null 访问。
            context.Loadout = new BattleLoadout();
            context.Rng = new BattleRng(0);
            context.TraitOfferService = new TraitOfferService(new List<TraitPoolEntry>());

            return context;
        }

        // ─── 私有辅助 ───────────────────────────────────────────────────────────

        private static string[] MergeTraitPoolIds(HeroDef? heroDef, WeaponDef? weaponDef)
        {
            List<string> ids = new();
            if (heroDef != null)
            {
                ids.AddRange(heroDef.TraitPoolIds);
            }
            if (weaponDef != null)
            {
                foreach (string id in weaponDef.TraitPoolIds)
                {
                    if (!ids.Contains(id))
                    {
                        ids.Add(id);
                    }
                }
            }
            return ids.ToArray();
        }

        /// <summary>内置默认武器：追踪弹、单枪口，供无武器注册时保持最小闭环。</summary>
        private static WeaponDef FallbackWeaponDef() => new("weapon_default")
        {
            BulletType = BulletType.Tracking,
            Damage = 10f,
            BulletSpeed = 12f,
            CooldownSeconds = 0.75f,
            Sockets = new[] { FireSocket.Default },
        };
    }
}
