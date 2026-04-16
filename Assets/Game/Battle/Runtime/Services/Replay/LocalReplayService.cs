using System;
using System.Collections.Generic;
using Game.Battle.Runtime.Commands;

namespace Game.Battle.Runtime.Services.Replay
{
    /// <summary>
    /// In-memory replay storage for minimal-loop validation.
    /// </summary>
    public sealed class LocalReplayService : IReplayService
    {
        private readonly Dictionary<int, List<IFrameCommand>> _records = new();

        public void RecordFrameCommands(int frame, IReadOnlyList<IFrameCommand> commands)
        {
            if (commands.Count == 0)
            {
                return;
            }

            if (!_records.TryGetValue(frame, out List<IFrameCommand> frameCommands))
            {
                frameCommands = new List<IFrameCommand>();
                _records.Add(frame, frameCommands);
            }

            frameCommands.AddRange(commands);
        }

        public IReadOnlyList<IFrameCommand> ReadFrameCommands(int frame)
        {
            if (_records.TryGetValue(frame, out List<IFrameCommand> frameCommands))
            {
                return frameCommands;
            }

            return Array.Empty<IFrameCommand>();
        }
    }
}
