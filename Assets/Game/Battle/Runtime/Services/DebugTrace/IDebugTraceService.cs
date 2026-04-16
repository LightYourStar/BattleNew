using Game.Battle.Runtime.Commands;

namespace Game.Battle.Runtime.Services.DebugTrace
{
    /// <summary>
    /// 调试追踪服务：把关键战斗链路变成可观测日志/采样点。
    /// <para>
    /// 目标：
    /// - 新人能快速判断“卡在哪一帧、哪条命令、哪个状态、哪次伤害”。
    /// - 后续可替换为文件、Telemetry、EditorWindow 可视化等实现。
    /// </para>
    /// </summary>
    public interface IDebugTraceService
    {
        /// <summary>逻辑帧推进（通常在固定 Tick 开始）。</summary>
        void TraceFrameAdvance(int frame);

        /// <summary>命令已被消费（注意：应与 FrameCommandBuffer 的 Drain 语义对齐）。</summary>
        void TraceCommandConsumed(IFrameCommand command);

        /// <summary>状态切换（AI/Hero/关卡流程等）。</summary>
        void TraceStateChange(string owner, string fromState, string toState);

        /// <summary>伤害结算（从命中到扣血的业务链路观测点）。</summary>
        void TraceDamage(string attackerId, string targetId, float amount);

        /// <summary>Buff 变更（Add/Remove/Refresh 等）。</summary>
        void TraceBuffChange(string ownerId, string buffId, string action);
    }
}
