using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Rules.StageHandlers
{
    /// <summary>
    /// 默认关卡流程：当“没有待处理波次”且“场上没有存活敌人”时认为关卡已清场。
    /// <para>
    /// 说明：这是为了最小闭环快速打通胜利链路；后续会引入更丰富的阶段条件与事件。
    /// </para>
    /// </summary>
    public sealed class DefaultStageHandler : IStageHandler
    {
        /// <inheritdoc />
        public bool IsStageCleared { get; private set; }

        /// <inheritdoc />
        public void Tick(BattleContext context, float deltaTime)
        {
            bool noPendingWave = context.WaveSystem.PendingWaveCount == 0;
            bool noAliveEnemy = context.Registry.Enemies.TrueForAll(enemy => !enemy.IsAlive);
            IsStageCleared = noPendingWave && noAliveEnemy;
        }
    }
}
