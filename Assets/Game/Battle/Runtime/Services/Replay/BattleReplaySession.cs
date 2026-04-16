using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Hero;
using Game.Battle.Runtime.Services.DebugTrace;
using UnityEngine;

namespace Game.Battle.Runtime.Services.Replay
{
    /// <summary>
    /// 回放会话：把 <see cref="ReplayRecord"/> 重新"喂"给一个全新的 <see cref="BattleWorld"/>，
    /// 走与正常战斗完全一致的命令消费链，不需要改动 BattleWorld 任何代码。
    /// <para>
    /// 使用流程：
    /// <code>
    /// // 1. 录制阶段结束后，导出录像
    /// ReplayRecord record = bootstrap.World.Context.ReplayService.ExportRecord();
    ///
    /// // 2. 创建回放会话
    /// BattleReplaySession session = new BattleReplaySession(record);
    /// session.Start();
    ///
    /// // 3. 每帧驱动
    /// session.Update(Time.deltaTime);
    ///
    /// // 4. 检查是否播放完毕
    /// if (session.IsFinished) { ... }
    /// </code>
    /// </para>
    /// </summary>
    public sealed class BattleReplaySession
    {
        private readonly ReplayRecord _record;
        private readonly BattleWorld _world;

        /// <summary>是否已经开始。</summary>
        public bool IsStarted { get; private set; }

        /// <summary>
        /// 是否已播完：当逻辑帧超过录像最大帧时置 true。
        /// </summary>
        public bool IsFinished { get; private set; }

        /// <summary>当前回放到的逻辑帧号。</summary>
        public int CurrentFrame => _world.Context.Time.Frame;

        /// <summary>录像总帧数（含无命令帧）。</summary>
        public int TotalFrames => _record.MaxFrame + 1;

        /// <summary>
        /// 回放速度倍率（1.0 = 原速，2.0 = 2 倍速，0.5 = 半速）。
        /// 调整后对 Update 传入的 deltaTime 做缩放。
        /// </summary>
        public float PlaybackSpeed { get; set; } = 1f;

        /// <summary>内部驱动的 BattleWorld（供可视化层只读访问）。</summary>
        public BattleWorld World => _world;

        /// <summary>
        /// 构造回放会话。
        /// </summary>
        /// <param name="record">录像数据（必须已 Seal）。</param>
        /// <param name="fixedStep">与录制时相同的逻辑帧步长，默认 1/30s。</param>
        /// <param name="debugTraceService">可选：传入追踪服务用于回放调试日志。</param>
        public BattleReplaySession(
            ReplayRecord record,
            float fixedStep = 1f / 30f,
            IDebugTraceService? debugTraceService = null)
        {
            _record = record;

            // ReplayNetAdapter 把录像命令伪装成"网络远端命令"注入 OrderBus，
            // BattleWorld 完全感知不到当前是回放还是正常战斗。
            ReplayNetAdapter replayAdapter = new(record);

            _world = new BattleWorld(
                fixedStep: fixedStep,
                netAdapter: replayAdapter,
                debugTraceService: debugTraceService ?? new NullDebugTraceService());
        }

        /// <summary>
        /// 启动回放：播种初始实体（与录制时一致），启动帧循环。
        /// </summary>
        public void Start()
        {
            if (IsStarted)
            {
                return;
            }

            // 和 BattleBootstrap.SeedMinimalLoop 保持一致
            _world.Context.Registry.Heroes.Add(new HeroEntity("hero_1", Vector3.zero));
            _world.Start();
            IsStarted = true;
        }

        /// <summary>
        /// 每渲染帧驱动：将 deltaTime 乘以速度倍率后交给 BattleWorld 推进逻辑帧。
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!IsStarted || IsFinished)
            {
                return;
            }

            _world.Update(deltaTime * PlaybackSpeed);

            // 当逻辑帧超过录像最大帧时，停止回放
            if (CurrentFrame > _record.MaxFrame)
            {
                Finish();
            }
        }

        /// <summary>暂停回放（暂停底层帧循环）。</summary>
        public void Pause()
        {
            _world.Pause();
        }

        /// <summary>继续回放（恢复底层帧循环）。</summary>
        public void Resume()
        {
            _world.Resume();
        }

        /// <summary>手动结束回放并停止底层世界。</summary>
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
