using Game.Battle.Runtime.Commands;
using UnityEngine;

namespace Game.Battle.Runtime.Services.DebugTrace
{
    /// <summary>
    /// Unity 控制台输出版追踪：适合在 Editor/Development Build 下快速肉眼验证。
    /// <para>
    /// 说明：
    /// - 默认只输出“高价值事件”（伤害/状态/Buff），避免刷屏。
    /// - 帧推进与命令消费属于高频事件，因此用开关控制。
    /// </para>
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

        /// <inheritdoc />
        public void TraceFrameAdvance(int frame)
        {
            if (_logFrameAdvance)
            {
                Debug.Log($"[Battle][Frame] -> {frame}");
            }
        }

        /// <inheritdoc />
        public void TraceCommandConsumed(IFrameCommand command)
        {
            if (_logCommandConsumed)
            {
                Debug.Log($"[Battle][Command] frame={command.Frame}, type={command.GetType().Name}, source={command.Source}");
            }
        }

        /// <inheritdoc />
        public void TraceStateChange(string owner, string fromState, string toState)
        {
            Debug.Log($"[Battle][State] owner={owner}, {fromState} -> {toState}");
        }

        /// <inheritdoc />
        public void TraceDamage(string attackerId, string targetId, float amount)
        {
            Debug.Log($"[Battle][Damage] {attackerId} -> {targetId}, amount={amount}");
        }

        /// <inheritdoc />
        public void TraceBuffChange(string ownerId, string buffId, string action)
        {
            Debug.Log($"[Battle][Buff] owner={ownerId}, buff={buffId}, action={action}");
        }
    }
}
