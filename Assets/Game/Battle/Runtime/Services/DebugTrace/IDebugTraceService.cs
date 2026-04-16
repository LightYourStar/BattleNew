using Game.Battle.Runtime.Commands;

namespace Game.Battle.Runtime.Services.DebugTrace
{
    public interface IDebugTraceService
    {
        void TraceFrameAdvance(int frame);

        void TraceCommandConsumed(IFrameCommand command);

        void TraceStateChange(string owner, string fromState, string toState);

        void TraceDamage(string attackerId, string targetId, float amount);

        void TraceBuffChange(string ownerId, string buffId, string action);
    }
}
