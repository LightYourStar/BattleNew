using System;
using System.Collections.Generic;
using Game.Battle.Runtime.Commands;

namespace Game.Battle.Runtime.Services.Replay
{
    /// <summary>
    /// 内存版回放记录器：用于最小闭环验证与本地调试。
    /// <para>
    /// 限制说明：
    /// - 这是"能跑起来"的占位实现，不做磁盘持久化、不做压缩、不做版本兼容。
    /// - 长时间运行会持续增长内存；后续应替换为分块文件/环形缓冲等实现。
    /// </para>
    /// </summary>
    public sealed class LocalReplayService : IReplayService
    {
        private readonly Dictionary<int, List<IFrameCommand>> _records = new();

        /// <summary>当前录制到的最大帧号（从未录到命令时为 -1）。</summary>
        private int _maxRecordedFrame = -1;

        /// <inheritdoc />
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

            if (frame > _maxRecordedFrame)
            {
                _maxRecordedFrame = frame;
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<IFrameCommand> ReadFrameCommands(int frame)
        {
            if (_records.TryGetValue(frame, out List<IFrameCommand> frameCommands))
            {
                return frameCommands;
            }

            return Array.Empty<IFrameCommand>();
        }

        /// <inheritdoc />
        public ReplayRecord ExportRecord()
        {
            ReplayRecord record = new();
            foreach (var kvp in _records)
            {
                record.AddFrame(new ReplayFrameData(kvp.Key, kvp.Value));
            }
            record.Seal(_maxRecordedFrame);
            return record;
        }
    }
}
