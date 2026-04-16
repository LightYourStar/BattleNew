using System.Collections.Generic;
using Game.Battle.Runtime.Services.DebugTrace;
using Game.Battle.Runtime.Services.Network;

namespace Game.Battle.Runtime.Commands
{
    /// <summary>
    /// 命令总线：把“写入缓冲、网络发送/拉取、调试追踪、缺帧检测”集中在一个入口。
    /// <para>
    /// 设计目标：
    /// - 让 BattleWorld 只关心“本帧要消费哪些命令”，而不关心命令从哪来。
    /// - 为后续网络帧同步与补帧预留统一扩展点。
    /// </para>
    /// </summary>
    public sealed class OrderBus
    {
        private readonly FrameCommandBuffer _commandBuffer;
        private readonly INetAdapter _netAdapter;
        private readonly IDebugTraceService _debugTraceService;

        public OrderBus(
            FrameCommandBuffer commandBuffer,
            INetAdapter netAdapter,
            IDebugTraceService debugTraceService)
        {
            _commandBuffer = commandBuffer;
            _netAdapter = netAdapter;
            _debugTraceService = debugTraceService;
        }

        /// <summary>
        /// 推送本地命令：写入缓冲，并尝试通过网络适配器发送（单机默认实现可为空操作）。
        /// </summary>
        public void PushLocalCommand(IFrameCommand command)
        {
            _commandBuffer.Add(command);
            _netAdapter.SendFrameCommand(command);
        }

        /// <summary>推送远端命令：仅写入缓冲（发送侧由网络层负责）。</summary>
        public void PushRemoteCommand(IFrameCommand command)
        {
            _commandBuffer.Add(command);
        }

        /// <summary>从网络适配器拉取远端命令并写入缓冲（在固定 Tick 早期调用）。</summary>
        public void PullRemoteCommands(int frame)
        {
            IReadOnlyList<IFrameCommand> remoteCommands = _netAdapter.PullRemoteCommands(frame);
            for (int i = 0; i < remoteCommands.Count; i++)
            {
                _commandBuffer.Add(remoteCommands[i]);
            }
        }

        /// <summary>
        /// 取出并消费指定帧命令：会触发调试追踪（命令消费可视化）。
        /// </summary>
        public IReadOnlyList<IFrameCommand> DequeueFrameCommands(int frame)
        {
            IReadOnlyList<IFrameCommand> commands = _commandBuffer.Drain(frame);
            for (int i = 0; i < commands.Count; i++)
            {
                _debugTraceService.TraceCommandConsumed(commands[i]);
            }

            return commands;
        }

        /// <summary>
        /// 判断指定帧是否“完全没有命令桶”。
        /// <para>
        /// 注意：当前实现是“没有桶就算缺帧”。后续网络帧同步通常需要引入更精细语义：
        /// - 某些帧可能允许空命令桶（合法空帧）
        /// - 某些帧必须等待远端确认（需要状态机/序号）
        /// </para>
        /// </summary>
        public bool HasMissingFrames(int frame)
        {
            return !_commandBuffer.HasFrame(frame);
        }

        /// <summary>
        /// 预留：补帧/重传请求入口（网络层实现后再接入）。
        /// </summary>
        public void RequestMissingFrame(int fromFrame, int toFrame)
        {
            // Reserved for future network retransmit request implementation.
        }
    }
}
