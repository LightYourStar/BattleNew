using System.Collections.Generic;

namespace Game.Battle.Runtime.Commands
{
    /// <summary>
    /// Stores commands by frame index for deterministic consumption.
    /// </summary>
    public sealed class FrameCommandBuffer
    {
        private readonly Dictionary<int, List<IFrameCommand>> _buffer = new();

        public void Add(IFrameCommand command)
        {
            if (!_buffer.TryGetValue(command.Frame, out List<IFrameCommand> frameCommands))
            {
                frameCommands = new List<IFrameCommand>();
                _buffer.Add(command.Frame, frameCommands);
            }

            frameCommands.Add(command);
        }

        public IReadOnlyList<IFrameCommand> Drain(int frame)
        {
            if (!_buffer.TryGetValue(frame, out List<IFrameCommand> frameCommands))
            {
                return System.Array.Empty<IFrameCommand>();
            }

            _buffer.Remove(frame);
            return frameCommands;
        }

        public bool HasFrame(int frame)
        {
            return _buffer.ContainsKey(frame);
        }
    }
}
