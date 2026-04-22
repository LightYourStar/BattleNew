using System.Collections.Generic;

namespace Game.Battle.Runtime.Entities.Trait
{
    /// <summary>
    /// 词条池中单条目：词条 Id + 权重。
    /// 权重越高在抽取时出现概率越大；全为 1 时退化为均匀随机。
    /// </summary>
    public readonly struct TraitPoolEntry
    {
        /// <summary>词条定义 Id（与 <see cref="TraitFactory"/> 注册的 key 对应）。</summary>
        public string TraitId { get; }

        /// <summary>抽取权重（正整数，默认 1）。</summary>
        public int Weight { get; }

        public TraitPoolEntry(string traitId, int weight = 1)
        {
            TraitId = traitId;
            Weight = weight;
        }
    }

    /// <summary>
    /// 词条池定义：一组可出现在局内升级界面的词条及其权重。
    /// <para>
    /// 示例：英雄 A 携带 "pool_warrior"，武器 B 携带 "pool_sword"；
    /// 两个池合并后即为本局三选一的候选集。
    /// </para>
    /// </summary>
    public sealed class TraitPoolDef
    {
        /// <summary>池唯一 Id（与 <see cref="BattleLoadout.TraitPoolIds"/> 中的元素对应）。</summary>
        public string PoolId { get; }

        private readonly List<TraitPoolEntry> _entries;

        /// <summary>只读词条条目视图。</summary>
        public IReadOnlyList<TraitPoolEntry> Entries => _entries;

        public TraitPoolDef(string poolId, IEnumerable<TraitPoolEntry> entries)
        {
            PoolId = poolId;
            _entries = new List<TraitPoolEntry>(entries);
        }

        /// <summary>便捷构造：传入 traitId 数组，权重全为 1。</summary>
        public static TraitPoolDef Uniform(string poolId, params string[] traitIds)
        {
            var entries = new TraitPoolEntry[traitIds.Length];
            for (int i = 0; i < traitIds.Length; i++)
            {
                entries[i] = new TraitPoolEntry(traitIds[i]);
            }
            return new TraitPoolDef(poolId, entries);
        }
    }
}
