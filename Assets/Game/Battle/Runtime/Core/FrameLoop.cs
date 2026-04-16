using System;

namespace Game.Battle.Runtime.Core
{
    /// <summary>
    /// 固定步长逻辑循环：把渲染帧（可变 deltaTime）与逻辑帧（固定 FixedStep）解耦。
    /// <para>
    /// 典型用法：
    /// - 渲染侧每帧调用 <see cref="Tick"/> 驱动累加器。
    /// - 逻辑侧订阅 <see cref="OnFixedTick"/>，只在固定步长回调里推进战斗模拟。
    /// </para>
    /// </summary>
    public sealed class FrameLoop
    {
        /// <summary>累加器：把可变帧时间切片为多个固定逻辑步长。</summary>
        private float _accumulator;

        /// <summary>固定逻辑步长（秒）。</summary>
        public float FixedStep { get; }

        /// <summary>是否处于运行状态（Stop 后为 false）。</summary>
        public bool IsRunning { get; private set; }

        /// <summary>是否暂停（暂停时不消耗累加器、不触发固定 Tick）。</summary>
        public bool IsPaused { get; private set; }

        /// <summary>固定逻辑帧回调：参数为 FixedStep（不是渲染 deltaTime）。</summary>
        public Action<float>? OnFixedTick;

        public FrameLoop(float fixedStep)
        {
            FixedStep = fixedStep;
        }

        /// <summary>启动循环：清空累加器并进入运行态。</summary>
        public void Start()
        {
            _accumulator = 0f;
            IsRunning = true;
            IsPaused = false;
        }

        /// <summary>暂停：渲染仍可继续，但逻辑固定 Tick 停止推进。</summary>
        public void Pause()
        {
            IsPaused = true;
        }

        /// <summary>恢复运行。</summary>
        public void Resume()
        {
            IsPaused = false;
        }

        /// <summary>停止：清理运行标记与累加器。</summary>
        public void Stop()
        {
            IsRunning = false;
            IsPaused = false;
            _accumulator = 0f;
        }

        /// <summary>
        /// 渲染帧驱动入口：根据 deltaTime 推进固定逻辑帧。
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!IsRunning || IsPaused)
            {
                return;
            }

            _accumulator += deltaTime;
            while (_accumulator >= FixedStep)
            {
                _accumulator -= FixedStep;
                OnFixedTick?.Invoke(FixedStep);
            }
        }
    }
}
