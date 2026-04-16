using Game.Battle.Runtime.Commands;

namespace Game.Battle.Runtime.Services.DebugTrace
{
    /// <summary>
    /// Default no-op trace sink for early skeleton stage.
    /// </summary>
    public sealed class NullDebugTraceService : IDebugTraceService
    {
        public void TraceBuffChange(string ownerId, string buffId, string action)
        {
        }

        public void TraceCommandConsumed(IFrameCommand command)
        {
        }

        public void TraceDamage(string attackerId, string targetId, float amount)
        {
        }

        public void TraceFrameAdvance(int frame)
        {
        }

        public void TraceStateChange(string owner, string fromState, string toState)
        {
        }
    }
}
