using System.Collections.Generic;
using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.AI;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.Wave
{
    /// <summary>
    /// 刷怪服务：从 WaveSystem 中拆出具体的怪物生成逻辑（位置、属性、数量），
    /// 使 WaveSystem 只关心"何时刷新"而不关心"怎么构造怪物"。
    /// </summary>
    public sealed class SpawnSystem
    {
        /// <summary>
        /// 生成一波怪物并返回创建出的实体列表（后续可扩展为读配置表）。
        /// </summary>
        public List<AIEntity> SpawnWave(BattleContext context, int waveIndex)
        {
            List<AIEntity> spawned = new();
            int count = 1 + waveIndex;
            for (int i = 0; i < count; i++)
            {
                float x = 4f + i * 2f;
                AIEntity enemy = new(
                    id: $"enemy_w{waveIndex}_{i}",
                    spawnPosition: new Vector3(x, 0f, 5f),
                    maxHp: 30f + waveIndex * 10f);
                spawned.Add(enemy);
            }

            return spawned;
        }
    }
}
