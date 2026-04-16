using Game.Battle.Runtime.Commands;
using UnityEngine;

namespace Game.Battle.Runtime.Services.DebugTrace
{
    /// <summary>
    /// Lightweight Unity logger for validating first-playable loop behavior.
    /// </summary>
    public sealed class UnityDebugTraceService : IDebugTraceService
    {
        private readonly bool _logFrameAdvance;
        private readonly bool _logCommandConsumed;

        public UnityDebugTraceService(bool logFrameAdvance = false, bool logCommandConsumed = false)
        {
            _logFrameAdvance = logFrameAdvance;
            _logCommandConsumed = logCommandConsumed;
        }

        public void TraceFrameAdvance(int frame)
        {
            if (_logFrameAdvance)
            {
                Debug.Log($"[Battle][Frame] -> {frame}");
            }
        }

        public void TraceCommandConsumed(IFrameCommand command)
        {
            if (_logCommandConsumed)
            {
                Debug.Log($"[Battle][Command] frame={command.Frame}, type={command.GetType().Name}, source={command.Source}");
            }
        }

        public void TraceStateChange(string owner, string fromState, string toState)
        {
            Debug.Log($"[Battle][State] owner={owner}, {fromState} -> {toState}");
        }

        public void TraceDamage(string attackerId, string targetId, float amount)
        {
            Debug.Log($"[Battle][Damage] {attackerId} -> {targetId}, amount={amount}");
        }

        public void TraceBuffChange(string ownerId, string buffId, string action)
        {
            Debug.Log($"[Battle][Buff] owner={ownerId}, buff={buffId}, action={action}");
        }
    }
}
