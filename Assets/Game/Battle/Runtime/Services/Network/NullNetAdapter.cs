using System;
using System.Collections.Generic;
using Game.Battle.Runtime.Commands;

namespace Game.Battle.Runtime.Services.Network
{
    /// <summary>
    /// Local-only adapter that does not provide remote commands.
    /// </summary>
    public sealed class NullNetAdapter : INetAdapter
    {
        public IReadOnlyList<IFrameCommand> PullRemoteCommands(int frame)
        {
            return Array.Empty<IFrameCommand>();
        }

        public void SendFrameCommand(IFrameCommand command)
        {
        }
    }
}
