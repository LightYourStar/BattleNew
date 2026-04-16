using System;
using System.Collections.Generic;
using Game.Battle.Runtime.Commands;
using Game.Battle.Runtime.Services.Network;

namespace Game.Battle.Runtime.Services.Replay
{
    /// <summary>
    /// 回放专用网络适配器：把"从 ReplayRecord 读命令"伪装成"网络远端命令"，
    /// 使 <see cref="OrderBus"/> 无需感知当前是正常战斗还是回放。
    /// <para>
    /// 工作原理：
    /// - <see cref="PullRemoteCommands"/> 在每帧被 OrderBus 调用时，
    ///   从 ReplayRecord 里取出该帧录制的命令返回，注入进 FrameCommandBuffer，
    ///   后续与正常战斗完全走同一条消费链。
    /// - <see cref="SendFrameCommand"/> 在回放模式下为空操作，不再向网络发送。
    /// </para>
    /// </summary>
    public sealed class ReplayNetAdapter : INetAdapter
    {
        private readonly ReplayRecord _record;

        public ReplayNetAdapter(ReplayRecord record)
        {
            _record = record;
        }

        /// <summary>回放模式下不向网络发送命令（空操作）。</summary>
        public void SendFrameCommand(IFrameCommand command)
        {
        }

        /// <summary>
        /// 返回 ReplayRecord 中该帧录制的命令列表；
        /// 无记录的帧（空命令帧）返回空集合，与正常战斗行为一致。
        /// </summary>
        public IReadOnlyList<IFrameCommand> PullRemoteCommands(int frame)
        {
            ReplayFrameData? frameData = _record.GetFrame(frame);
            return frameData?.Commands ?? Array.Empty<IFrameCommand>();
        }
    }
}
