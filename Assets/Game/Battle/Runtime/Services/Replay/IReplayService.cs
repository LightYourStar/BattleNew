using System.Collections.Generic;
using Game.Battle.Runtime.Commands;

namespace Game.Battle.Runtime.Services.Replay
{
    public interface IReplayService
    {
        void RecordFrameCommands(int frame, IReadOnlyList<IFrameCommand> commands);

        IReadOnlyList<IFrameCommand> ReadFrameCommands(int frame);
    }
}
