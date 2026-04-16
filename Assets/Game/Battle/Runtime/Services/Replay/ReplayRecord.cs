using System.Collections.Generic;

namespace Game.Battle.Runtime.Services.Replay
{
    /// <summary>
    /// 一局完整录像：包含所有有命令的帧数据，可交给 <see cref="BattleReplaySession"/> 回放。
    /// <para>
    /// 说明：只有"存在至少一条命令"的帧才会有对应 <see cref="ReplayFrameData"/>；
    /// 空命令帧不记录，回放时自然跳过（行为与正常战斗一致）。
    /// </para>
    /// </summary>
    public sealed class ReplayRecord
    {
        private readonly List<ReplayFrameData> _frames = new();

        /// <summary>所有有命令帧的数据列表（只读视图）。</summary>
        public IReadOnlyList<ReplayFrameData> Frames => _frames;

        /// <summary>有命令帧的数量（不等于总帧数）。</summary>
        public int CommandFrameCount => _frames.Count;

        /// <summary>
        /// 整场录像持续的最大帧号（由 <see cref="Seal"/> 写入）。
        /// 回放方用此值判断何时结束。
        /// </summary>
        public int MaxFrame { get; private set; } = -1;

        /// <summary>录像是否已封存（封存后才可安全用于回放）。</summary>
        public bool IsSealed { get; private set; }

        /// <summary>添加一帧数据（仅在录制阶段调用）。</summary>
        public void AddFrame(ReplayFrameData frameData)
        {
            _frames.Add(frameData);
        }

        /// <summary>
        /// 封存录像：记录最大帧号，之后该对象应视为只读。
        /// </summary>
        public void Seal(int maxFrame)
        {
            MaxFrame = maxFrame;
            IsSealed = true;
        }

        /// <summary>
        /// 按帧号查找对应帧数据；无记录（空命令帧）时返回 null。
        /// </summary>
        public ReplayFrameData? GetFrame(int frame)
        {
            for (int i = 0; i < _frames.Count; i++)
            {
                if (_frames[i].Frame == frame)
                {
                    return _frames[i];
                }
            }
            return null;
        }
    }
}
