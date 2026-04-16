namespace Game.Battle.Runtime.Core
{
    /// <summary>
    /// Tracks deterministic battle frame timing.
    /// </summary>
    public sealed class BattleTime
    {
        public int Frame { get; private set; }

        public float FixedDeltaTime { get; }

        public float ElapsedTime => Frame * FixedDeltaTime;

        public BattleTime(float fixedDeltaTime)
        {
            FixedDeltaTime = fixedDeltaTime;
            Frame = 0;
        }

        public void AdvanceOneFrame()
        {
            Frame++;
        }

        public void Reset()
        {
            Frame = 0;
        }
    }
}
