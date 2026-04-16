using Game.Battle.Runtime.Commands;

namespace Game.Battle.Runtime.Services.DebugTrace
{
    /// <summary>
    /// 默认空实现：避免骨架阶段到处判空。
    /// </summary>
    public sealed class NullDebugTraceService : IDebugTraceService
    {
        /// <inheritdoc />
        public void TraceBuffChange(string ownerId, string buffId, string action)
        {
        }

        /// <inheritdoc />
        public void TraceCommandConsumed(IFrameCommand command)
        {
        }

        /// <inheritdoc />
        public void TraceDamage(string attackerId, string targetId, float amount)
        {
        }

        /// <inheritdoc />
        public void TraceFrameAdvance(int frame)
        {
        }

        /// <inheritdoc />
        public void TraceStateChange(string owner, string fromState, string toState)
        {
        }
    }
}
