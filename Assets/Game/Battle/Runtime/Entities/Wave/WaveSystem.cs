using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.AI;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.Wave
{
    /// <summary>
    /// Spawns a minimal enemy wave for first playable loop.
    /// </summary>
    public sealed class WaveSystem
    {
        private bool _spawned;

        public int PendingWaveCount { get; private set; } = 1;

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
