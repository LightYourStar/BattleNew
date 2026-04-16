namespace Game.Battle.Runtime.Commands
{
    /// <summary>
    /// 命令来源：用于回放、网络同步与本地输入的统一标注。
    /// </summary>
    public enum CommandSource
    {
        /// <summary>本机输入采集。</summary>
        LocalInput = 0,

        /// <summary>网络对端回推的命令。</summary>
        RemoteSync = 1,

        /// <summary>回放重放注入的命令。</summary>
        Replay = 2,
    }

    /// <summary>
    /// 强类型逻辑帧命令的根接口。
    /// <para>
    /// 约束：
    /// - 命令必须携带 Frame，用于与固定逻辑帧消费对齐。
    /// - 禁止用弱语义字段（例如 PM1/PM2）承载复杂语义；应拆成独立命令类型。
    /// </para>
    /// </summary>
    public interface IFrameCommand
    {
        /// <summary>该命令应当在哪一逻辑帧被消费。</summary>
        int Frame { get; }

        /// <summary>命令来源（本地/远端/回放）。</summary>
        CommandSource Source { get; }
    }
}
