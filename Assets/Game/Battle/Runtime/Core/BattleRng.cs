using System;

namespace Game.Battle.Runtime.Core
{
    /// <summary>
    /// 确定性随机数生成器：用同一个种子可以在任意两台机器上复现完全相同的随机序列。
    /// <para>
    /// 设计原则：
    /// <list type="bullet">
    ///   <item>所有战斗内随机（词条 Offer 抽取、刷怪位置偏移等）只通过此实例取随机数。</item>
    ///   <item>渲染/表现层的随机（粒子、音效、镜头抖动）不使用此实例，不影响逻辑确定性。</item>
    ///   <item>种子存入 <see cref="BattleLoadout.RngSeed"/>，并随 <see cref="Services.Replay.ReplayRecord"/> 一起持久化，
    ///         从而保证回放时的随机序列与录制时完全一致。</item>
    /// </list>
    /// </para>
    /// </summary>
    public sealed class BattleRng
    {
        private readonly Random _random;

        /// <summary>构造本实例时使用的种子（用于序列化 / 调试）。</summary>
        public long Seed { get; }

        public BattleRng(long seed)
        {
            Seed = seed;
            // System.Random 的构造接受 int；对 long 取低 32 位再异或高 32 位，避免大种子丢失信息
            int intSeed = (int)(seed ^ (seed >> 32));
            _random = new Random(intSeed);
        }

        /// <summary>返回 [minInclusive, maxExclusive) 范围内的整数。</summary>
        public int Next(int minInclusive, int maxExclusive)
            => _random.Next(minInclusive, maxExclusive);

        /// <summary>返回 [0, maxExclusive) 范围内的整数。</summary>
        public int Next(int maxExclusive)
            => _random.Next(maxExclusive);

        /// <summary>返回 [0.0, 1.0) 范围内的浮点数。</summary>
        public float NextFloat()
            => (float)_random.NextDouble();

        /// <summary>
        /// 用加权随机从索引 [0, weights.Length) 中选一个。
        /// <para>权重全为零时退化为均匀随机。</para>
        /// </summary>
        public int NextWeighted(int[] weights)
        {
            int total = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                total += weights[i];
            }

            if (total <= 0)
            {
                return _random.Next(weights.Length);
            }

            int roll = _random.Next(total);
            int cumulative = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                {
                    return i;
                }
            }

            return weights.Length - 1;
        }
    }
}
