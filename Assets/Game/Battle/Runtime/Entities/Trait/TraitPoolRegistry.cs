using System.Collections.Generic;

namespace Game.Battle.Runtime.Entities.Trait
{
    /// <summary>
    /// 词条池注册表：运行时按池 Id 查询 <see cref="TraitPoolDef"/>。
    /// <para>
    /// 注册时机：在 <c>BattleBootstrap.EnterBattle(setupContext)</c> 的回调中，
    /// 由 Hotfix 层调用 <see cref="Register"/>，与 <see cref="TraitFactory"/> 的注册时机相同。
    /// </para>
    /// </summary>
    public sealed class TraitPoolRegistry
    {
        private readonly Dictionary<string, TraitPoolDef> _pools = new();

        public void Register(TraitPoolDef pool)
        {
            _pools[pool.PoolId] = pool;
        }

        /// <summary>按 Id 查找词条池；未注册时返回 null。</summary>
        public TraitPoolDef? Get(string poolId)
            => _pools.TryGetValue(poolId, out TraitPoolDef? def) ? def : null;

        /// <summary>
        /// 将多个池合并为一个词条条目列表（去重：同一 TraitId 取最高权重）。
        /// </summary>
        public List<TraitPoolEntry> Resolve(string[] poolIds)
        {
            Dictionary<string, int> merged = new();
            foreach (string poolId in poolIds)
            {
                TraitPoolDef? pool = Get(poolId);
                if (pool == null)
                {
                    continue;
                }

                foreach (TraitPoolEntry entry in pool.Entries)
                {
                    if (!merged.TryGetValue(entry.TraitId, out int existing) || entry.Weight > existing)
                    {
                        merged[entry.TraitId] = entry.Weight;
                    }
                }
            }

            List<TraitPoolEntry> result = new(merged.Count);
            foreach (KeyValuePair<string, int> kv in merged)
            {
                result.Add(new TraitPoolEntry(kv.Key, kv.Value));
            }
            return result;
        }
    }
}
