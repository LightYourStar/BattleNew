using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Rules.StageHandlers
{
    /// <summary>
    /// Minimal stage flow: clear when all spawned enemies are dead and no pending wave.
    /// </summary>
    public sealed class DefaultStageHandler : IStageHandler
    {
        public bool IsStageCleared { get; private set; }

        public void Tick(BattleContext context, float deltaTime)
        {
            bool noPendingWave = context.WaveSystem.PendingWaveCount == 0;
            bool noAliveEnemy = context.Registry.Enemies.TrueForAll(enemy => !enemy.IsAlive);
            IsStageCleared = noPendingWave && noAliveEnemy;
        }
    }
}
