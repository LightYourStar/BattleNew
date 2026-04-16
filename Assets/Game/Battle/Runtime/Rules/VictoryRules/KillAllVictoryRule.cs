using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Rules.VictoryRules
{
    /// <summary>
    /// Declares victory when stage is cleared.
    /// </summary>
    public sealed class KillAllVictoryRule : IVictoryRule
    {
        public bool IsBattleFinished { get; private set; }

        public bool IsVictory { get; private set; }

        public void Tick(BattleContext context, float deltaTime)
        {
            if (!context.StageHandler.IsStageCleared)
            {
                return;
            }

            IsBattleFinished = true;
            IsVictory = true;
        }
    }
}
