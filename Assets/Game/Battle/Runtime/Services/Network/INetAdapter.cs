using System.Collections.Generic;
using Game.Battle.Runtime.Commands;

namespace Game.Battle.Runtime.Services.Network
{
    public interface INetAdapter
    {
        void SendFrameCommand(IFrameCommand command);

        IReadOnlyList<IFrameCommand> PullRemoteCommands(int frame);
    }
}
