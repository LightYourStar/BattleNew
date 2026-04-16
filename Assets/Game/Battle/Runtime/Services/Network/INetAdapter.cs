using System.Collections.Generic;
using Game.Battle.Runtime.Commands;

namespace Game.Battle.Runtime.Services.Network
{
    /// <summary>
    /// 网络适配器：把“命令发送/拉取”从 OrderBus 中解耦出来，便于替换实现（单机/联机/回放）。
    /// </summary>
    public interface INetAdapter
    {
        /// <summary>
        /// 发送一帧命令到网络层（具体语义由实现决定：可立即发送、可合并、可缓存）。
        /// </summary>
        void SendFrameCommand(IFrameCommand command);

        /// <summary>
        /// 拉取指定逻辑帧应当并入缓冲区的远端命令。
        /// </summary>
        IReadOnlyList<IFrameCommand> PullRemoteCommands(int frame);
    }
}
