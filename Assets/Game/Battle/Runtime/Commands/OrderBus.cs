using System.Collections.Generic;
using Game.Battle.Runtime.Services.DebugTrace;
using Game.Battle.Runtime.Services.Network;

namespace Game.Battle.Runtime.Commands
{
    /// <summary>
    /// Central command bus for local, remote and replay command flow.
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

        public void PushLocalCommand(IFrameCommand command)
        {
            _commandBuffer.Add(command);
            _netAdapter.SendFrameCommand(command);
        }

        public void PushRemoteCommand(IFrameCommand command)
        {
            _commandBuffer.Add(command);
        }

        public void PullRemoteCommands(int frame)
        {
            IReadOnlyList<IFrameCommand> remoteCommands = _netAdapter.PullRemoteCommands(frame);
            for (int i = 0; i < remoteCommands.Count; i++)
            {
                _commandBuffer.Add(remoteCommands[i]);
            }
        }

        public IReadOnlyList<IFrameCommand> DequeueFrameCommands(int frame)
        {
            IReadOnlyList<IFrameCommand> commands = _commandBuffer.Drain(frame);
            for (int i = 0; i < commands.Count; i++)
            {
                _debugTraceService.TraceCommandConsumed(commands[i]);
            }

            return commands;
        }

        public bool HasMissingFrames(int frame)
        {
            return !_commandBuffer.HasFrame(frame);
        }

        public void RequestMissingFrame(int fromFrame, int toFrame)
        {
            // Reserved for future network retransmit request implementation.
        }
    }
}
