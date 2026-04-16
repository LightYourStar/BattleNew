using System.Collections.Generic;
using Game.Battle.Runtime.Commands;

namespace Game.Battle.Runtime.Services.Replay
{
    /// <summary>
    /// 单帧回放数据：帧号 + 该帧所有命令。
    /// </summary>
    public sealed class ReplayFrameData
    {
        public int Frame { get; }
        public IReadOnlyList<IFrameCommand> Commands { get; }

        public ReplayFrameData(int frame, IReadOnlyList<IFrameCommand> commands)
        {
            Frame = frame;
            Commands = commands;
        }
    }
}
