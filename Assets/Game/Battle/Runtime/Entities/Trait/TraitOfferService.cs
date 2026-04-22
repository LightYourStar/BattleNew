using System.Collections.Generic;
using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Entities.Trait
{
    /// <summary>
    /// 词条 Offer 服务实现：从本局合并后的词条池中，用确定性 RNG 加权无放回抽取 N 个词条 Id。
    /// <para>
    /// 算法（加权无放回）：
    /// <list type="number">
    ///   <item>将已装备词条从候选列表中排除（可选：允许升级叠层时跳过此步）。</item>
    ///   <item>反复调用 <see cref="BattleRng.NextWeighted"/> 选出一个词条，选后临时移除，
    ///         直到取够 count 个或候选耗尽。</item>
    /// </list>
    /// </para>
    /// </summary>
    public sealed class TraitOfferService : ITraitOfferService
    {
        private readonly List<TraitPoolEntry> _pool;

        /// <summary>
        /// 构造服务，传入本局已解析合并后的词条池（由 <see cref="TraitPoolRegistry.Resolve"/> 生成）。
        /// </summary>
        public TraitOfferService(List<TraitPoolEntry> resolvedPool)
        {
            _pool = resolvedPool;
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GenerateOffer(BattleContext context, int count = 3)
        {
            BattleRng rng = context.Rng;
            IReadOnlyList<ITrait> equipped = context.TraitSystem.Registry.ActiveTraits;

            // 构建候选列表（排除已装备词条，避免重复选同一词条）
            List<TraitPoolEntry> candidates = new(_pool.Count);
            foreach (TraitPoolEntry entry in _pool)
            {
                if (!IsEquipped(equipped, entry.TraitId))
                {
                    candidates.Add(entry);
                }
            }

            List<string> result = new(count);
            int[] weights = new int[candidates.Count];

            while (result.Count < count && candidates.Count > 0)
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    weights[i] = candidates[i].Weight;
                }

                // 用 BattleRng.NextWeighted 保证确定性
                int pick = rng.NextWeighted(weights);
                result.Add(candidates[pick].TraitId);
                candidates.RemoveAt(pick);

                // 同步收缩权重数组（下轮循环前重建，无需手动同步）
            }

            return result;
        }

        private static bool IsEquipped(IReadOnlyList<ITrait> equipped, string traitId)
        {
            for (int i = 0; i < equipped.Count; i++)
            {
                if (equipped[i].TraitId == traitId)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
