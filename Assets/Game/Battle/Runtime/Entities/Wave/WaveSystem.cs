using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.AI;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.Wave
{
    /// <summary>
    /// 波次系统：最小闭环下只刷一波怪，用于验证“刷新 -> 战斗 -> 清空波次计数”。
    /// <para>
    /// 这不是最终波次框架：后续会引入波次表、刷怪点、延迟刷新、精英规则等。
    /// </para>
    /// </summary>
    public sealed class WaveSystem
    {
        /// <summary>是否已经刷出本关第一波（当前示例只刷一次）。</summary>
        private bool _spawned;

        /// <summary>
        /// 仍待完成的波次数（最小示例：开局为 1；敌人全灭后置 0）。
        /// </summary>
        public int PendingWaveCount { get; private set; } = 1;

        /// <summary>每逻辑帧更新：负责生成敌人并维护 PendingWaveCount。</summary>
        public void Tick(BattleContext context, float deltaTime)
        {
            if (_spawned)
            {
                if (context.Registry.Enemies.TrueForAll(enemy => !enemy.IsAlive))
                {
                    PendingWaveCount = 0;
                }

                return;
            }

            context.Registry.Enemies.Add(new AIEntity("enemy_1", new Vector3(5f, 0f, 5f), 30f));
            _spawned = true;
        }
    }
}
