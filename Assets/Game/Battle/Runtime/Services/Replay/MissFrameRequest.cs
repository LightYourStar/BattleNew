namespace Game.Battle.Runtime.Services.Replay
{
    /// <summary>
    /// 补帧请求：表达"我缺了从 FromFrame 到 ToFrame 的数据"的网络消息。
    /// </summary>
    public sealed class MissFrameRequest
    {
        public int FromFrame { get; }
        public int ToFrame { get; }

        public MissFrameRequest(int fromFrame, int toFrame)
        {
            FromFrame = fromFrame;
            ToFrame = toFrame;
        }
    }
}
