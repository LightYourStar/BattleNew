namespace Game.Battle.Runtime.Entities.Wave
{
    /// <summary>
    /// 单波次配置数据（可对接配置表 / ScriptableObject）。
    /// </summary>
    public sealed class WaveConfig
    {
        /// <summary>波次序号（0-based）。</summary>
        public int WaveIndex { get; }

        /// <summary>此波次在上一波清空后等待多少秒再刷新（0 = 立即刷新）。</summary>
        public float DelaySeconds { get; }

        public WaveConfig(int waveIndex, float delaySeconds = 0f)
        {
            WaveIndex = waveIndex;
            DelaySeconds = delaySeconds;
        }
    }
}
