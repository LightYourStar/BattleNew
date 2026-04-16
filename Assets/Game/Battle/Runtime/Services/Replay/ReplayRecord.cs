using System.Collections.Generic;

namespace Game.Battle.Runtime.Services.Replay
{
    /// <summary>
    /// 一局完整录像：包含所有帧数据，可用于回放。
    /// </summary>
    public sealed class ReplayRecord
    {
        public List<ReplayFrameData> Frames { get; } = new();

        public int TotalFrames => Frames.Count;

        public void AddFrame(ReplayFrameData frameData)
        {
            Frames.Add(frameData);
        }

        public ReplayFrameData? GetFrame(int frame)
        {
            for (int i = 0; i < Frames.Count; i++)
            {
                if (Frames[i].Frame == frame)
                {
                    return Frames[i];
                }
            }
            return null;
        }
    }
}
