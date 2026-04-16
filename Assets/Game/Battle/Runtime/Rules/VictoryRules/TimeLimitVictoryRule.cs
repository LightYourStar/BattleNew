using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Rules.VictoryRules
{
    /// <summary>
    /// 限时胜利规则：在时间到达前存活即为胜利，超时则失败。
    /// </summary>
    public sealed class TimeLimitVictoryRule : IVictoryRule
    {
        private readonly float _timeLimitSeconds;

        public bool IsBattleFinished { get; private set; }
        public bool IsVictory { get; private set; }

        public TimeLimitVictoryRule(float timeLimitSeconds)
        {
            _timeLimitSeconds = timeLimitSeconds;
        }

        public void Tick(BattleContext context, float deltaTime)
        {
            if (context.StageHandler.IsStageCleared)
            {
                IsBattleFinished = true;
                IsVictory = true;
                return;
            }

            if (context.Time.ElapsedTime >= _timeLimitSeconds)
            {
                IsBattleFinished = true;
                IsVictory = false;
            }
        }
    }
}
