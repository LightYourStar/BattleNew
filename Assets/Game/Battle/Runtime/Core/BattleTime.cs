namespace Game.Battle.Runtime.Core
{
    /// <summary>
    /// 战斗逻辑时间轴：用“逻辑帧号 + 固定步长”表达确定性推进。
    /// <para>
    /// 设计意图：
    /// - 逻辑层只依赖 Frame，而不是依赖 Unity 的 Time.time（避免浮点抖动与不同机器差异）。
    /// - ElapsedTime 仅用于调试/展示，不作为强一致性依据。
    /// </para>
    /// </summary>
    public sealed class BattleTime
    {
        /// <summary>当前逻辑帧号（从 0 开始）。</summary>
        public int Frame { get; private set; }

        /// <summary>固定逻辑步长（秒），例如 1/30。</summary>
        public float FixedDeltaTime { get; }

        /// <summary>累计逻辑时间（秒），由 Frame 推导。</summary>
        public float ElapsedTime => Frame * FixedDeltaTime;

        public BattleTime(float fixedDeltaTime)
        {
            FixedDeltaTime = fixedDeltaTime;
            Frame = 0;
        }

        /// <summary>推进一帧（通常在 BattleWorld 的一次固定 Tick 末尾调用）。</summary>
        public void AdvanceOneFrame()
        {
            Frame++;
        }

        /// <summary>重置时间轴（例如重开一局或回放从头开始）。</summary>
        public void Reset()
        {
            Frame = 0;
        }
    }
}
