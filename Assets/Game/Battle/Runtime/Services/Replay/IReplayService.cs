using System.Collections.Generic;
using Game.Battle.Runtime.Commands;

namespace Game.Battle.Runtime.Services.Replay
{
    /// <summary>
    /// 回放服务：围绕“命令帧”进行录制与读取。
    /// <para>
    /// 重要原则（对齐迁移文档）：
    /// - 录制应尽量记录命令，而不是记录表现层结果。
    /// - 回放应走与正常战斗一致的命令消费链（BattleWorld -> OrderBus）。
    /// </para>
    /// </summary>
    public interface IReplayService
    {
        /// <summary>记录某一逻辑帧实际消费到的命令集合。</summary>
        void RecordFrameCommands(int frame, IReadOnlyList<IFrameCommand> commands);

        /// <summary>读取某一逻辑帧已记录的命令集合（回放注入使用）。</summary>
        IReadOnlyList<IFrameCommand> ReadFrameCommands(int frame);
    }
}
