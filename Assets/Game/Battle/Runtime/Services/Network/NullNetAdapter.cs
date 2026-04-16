using System;
using System.Collections.Generic;
using Game.Battle.Runtime.Commands;

namespace Game.Battle.Runtime.Services.Network
{
    /// <summary>
    /// 单机默认网络适配器：不发送、不拉取远端命令。
    /// <para>
    /// 用途：让 OrderBus 在单机模式下仍保持同一套调用路径，避免到处写 if (online)。
    /// </para>
    /// </summary>
    public sealed class NullNetAdapter : INetAdapter
    {
        /// <inheritdoc />
        public IReadOnlyList<IFrameCommand> PullRemoteCommands(int frame)
        {
            return Array.Empty<IFrameCommand>();
        }

        /// <inheritdoc />
        public void SendFrameCommand(IFrameCommand command)
        {
        }
    }
}
