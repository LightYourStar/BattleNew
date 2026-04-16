using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Rules.StageHandlers
{
    /// <summary>
    /// 多波次关卡流程：每清完一波推进下一波，直到所有波次完成。
    /// </summary>
    public sealed class MultiWaveStageHandler : IStageHandler
    {
        private readonly int _totalWaves;
        private int _currentWave;

        public bool IsStageCleared { get; private set; }

        public int CurrentWave => _currentWave;

        public MultiWaveStageHandler(int totalWaves)
        {
            _totalWaves = totalWaves;
        }

        public void Tick(BattleContext context, float deltaTime)
        {
            if (IsStageCleared)
            {
                return;
            }

            bool noPendingWave = context.WaveSystem.PendingWaveCount == 0;
            bool noAliveEnemy = context.Registry.Enemies.TrueForAll(enemy => !enemy.IsAlive);

            if (!noPendingWave || !noAliveEnemy)
            {
                return;
            }

            _currentWave++;
            if (_currentWave >= _totalWaves)
            {
                IsStageCleared = true;
            }
        }
    }
}
