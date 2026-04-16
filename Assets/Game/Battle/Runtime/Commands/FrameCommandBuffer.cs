using System.Collections.Generic;

namespace Game.Battle.Runtime.Commands
{
    /// <summary>
    /// 按逻辑帧索引缓存命令的缓冲区。
    /// <para>
    /// 典型用法：
    /// - 生产者：在帧 N 写入属于帧 N 的命令（本地输入/远端回推/回放注入）。
    /// - 消费者：在帧 N 的固定 Tick 中 Drain(N)，一次性取出并消费。
    /// </para>
    /// </summary>
    public sealed class FrameCommandBuffer
    {
        private readonly Dictionary<int, List<IFrameCommand>> _buffer = new();

        /// <summary>追加一条命令到指定帧的桶中（同一帧允许多条命令）。</summary>
        public void Add(IFrameCommand command)
        {
            if (!_buffer.TryGetValue(command.Frame, out List<IFrameCommand> frameCommands))
            {
                frameCommands = new List<IFrameCommand>();
                _buffer.Add(command.Frame, frameCommands);
            }

            frameCommands.Add(command);
        }

        /// <summary>
        /// 取出并移除指定帧的全部命令。
        /// <para>
        /// 注意：这是“取出即删除”的语义，避免同一帧命令被重复消费。
        /// </para>
        /// </summary>
        public IReadOnlyList<IFrameCommand> Drain(int frame)
        {
            if (!_buffer.TryGetValue(frame, out List<IFrameCommand> frameCommands))
            {
                return System.Array.Empty<IFrameCommand>();
            }

            _buffer.Remove(frame);
            return frameCommands;
        }

        /// <summary>判断指定帧是否仍有未消费的命令桶。</summary>
        public bool HasFrame(int frame)
        {
            return _buffer.ContainsKey(frame);
        }
    }
}
