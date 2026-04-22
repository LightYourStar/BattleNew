using System;
using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Services.DebugTrace;

namespace Game.Battle.Runtime.Services.Replay
{
    /// <summary>
    /// 回放会话：把 <see cref="ReplayRecord"/> 重新"喂"给一个全新的 <see cref="BattleWorld"/>，
    /// 走与正常战斗完全一致的命令消费链，保证逻辑结果可完全复现。
    /// <para>
    /// 关键设计：
    /// <list type="bullet">
    ///   <item><see cref="ReplayNetAdapter"/> 把录像命令伪装成"远端命令"注入 OrderBus，
    ///         BattleWorld 感知不到当前是回放还是正常战斗。</item>
    ///   <item>初始状态由 <see cref="ReplayRecord.Loadout"/> 还原（英雄/武器/RNG 种子），
    ///         保证与录制时完全一致。</item>
    ///   <item><paramref name="setupContext"/> 回调与录制时相同，确保注册表（HeroDef/WeaponDef/TraitFactory 等）
    ///         在回放世界中同样可用。</item>
    /// </list>
    /// </para>
    /// </summary>
    public sealed class BattleReplaySession
    {
        private readonly ReplayRecord _record;
        private readonly BattleWorld _world;
        private readonly Action<BattleContext>? _setupContext;

        public bool IsStarted { get; private set; }
        public bool IsFinished { get; private set; }

        /// <summary>当前回放到的逻辑帧号。</summary>
        public int CurrentFrame => _world.Context.Time.Frame;

        /// <summary>录像总帧数（含无命令帧）。</summary>
        public int TotalFrames => _record.MaxFrame + 1;

        /// <summary>回放速度倍率（1.0 = 原速，2.0 = 双倍速，0.5 = 半速）。</summary>
        public float PlaybackSpeed { get; set; } = 1f;

        /// <summary>内部驱动的 BattleWorld（供可视化层只读访问）。</summary>
        public BattleWorld World => _world;

        /// <summary>
        /// 构造回放会话。
        /// </summary>
        /// <param name="record">已封存的录像数据。</param>
        /// <param name="fixedStep">与录制时相同的逻辑帧步长，默认 1/30s。</param>
        /// <param name="debugTraceService">可选：回放调试日志服务。</param>
        /// <param name="setupContext">
        /// 可选：与录制时相同的注册回调（注册 HeroDef / WeaponDef / TraitFactory / TraitPool 等）。
        /// 若省略，词条命令等依赖注册表的操作在回放时会静默失败（与录制行为不一致）。
        /// </param>
        public BattleReplaySession(
            ReplayRecord record,
            float fixedStep = 1f / 30f,
            IDebugTraceService? debugTraceService = null,
            Action<BattleContext>? setupContext = null)
        {
            _record = record;
            _setupContext = setupContext;

            ReplayNetAdapter replayAdapter = new(record);

            _world = new BattleWorld(
                fixedStep: fixedStep,
                netAdapter: replayAdapter,
                debugTraceService: debugTraceService ?? new NullDebugTraceService());
        }

        /// <summary>
        /// 启动回放：还原注册表（setupContext）→ 按 Loadout 播种实体 → 启动帧循环。
        /// </summary>
        public void Start()
        {
            if (IsStarted)
            {
                return;
            }

            // 1. 先填充注册表（与录制时相同的 setupContext）
            _setupContext?.Invoke(_world.Context);

            // 2. 用录像携带的 Loadout 播种（还原英雄/武器/RNG 种子）
            BattleLoadout loadout = _record.Loadout ?? new BattleLoadout();
            _world.SeedFromLoadout(loadout);

            // 3. 启动帧循环
            _world.Start();
            IsStarted = true;
        }

        /// <summary>每渲染帧驱动：将 deltaTime 乘以速度倍率后交给 BattleWorld 推进逻辑帧。</summary>
        public void Update(float deltaTime)
        {
            if (!IsStarted || IsFinished)
            {
                return;
            }

            _world.Update(deltaTime * PlaybackSpeed);

            if (CurrentFrame > _record.MaxFrame)
            {
                Finish();
            }
        }

        public void Pause() => _world.Pause();
        public void Resume() => _world.Resume();

        public void Finish()
        {
            if (IsFinished)
            {
                return;
            }

            IsFinished = true;
            _world.Stop();
        }
    }
}
