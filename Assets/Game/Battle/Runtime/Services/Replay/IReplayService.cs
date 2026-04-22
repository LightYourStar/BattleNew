using System.Collections.Generic;
using Game.Battle.Runtime.Commands;
using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Services.Replay
{
    /// <summary>
    /// 回放服务：围绕"命令帧"进行录制与读取。
    /// <para>
    /// 重要原则（对齐迁移文档）：
    /// - 录制应尽量记录命令，而不是记录表现层结果。
    /// - 回放应走与正常战斗一致的命令消费链（BattleWorld → OrderBus）。
    /// </para>
    /// </summary>
    public interface IReplayService
    {
        /// <summary>记录某一逻辑帧实际消费到的命令集合。</summary>
        void RecordFrameCommands(int frame, IReadOnlyList<IFrameCommand> commands);

        /// <summary>读取某一逻辑帧已记录的命令集合（回放注入使用）。</summary>
        IReadOnlyList<IFrameCommand> ReadFrameCommands(int frame);

        /// <summary>
        /// 导出完整录像：把当前已录制的所有帧数据打包为 <see cref="ReplayRecord"/>，
        /// 用于持久化存储或交给 <see cref="BattleReplaySession"/> 回放。
        /// </summary>
        ReplayRecord ExportRecord();

        /// <summary>
        /// 将本局 <see cref="BattleLoadout"/> 写入录像（在战斗开始时调用）。
        /// 回放时 <see cref="BattleReplaySession"/> 从 <see cref="ReplayRecord.Loadout"/> 读取，
        /// 确保初始状态（英雄属性、武器、RNG 种子）与录制时完全一致。
        /// </summary>
        void SetLoadout(BattleLoadout loadout);
    }
}
