using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Rules.VictoryRules
{
    public interface IVictoryRule
    {
        bool IsBattleFinished { get; }

        bool IsVictory { get; }

        void Tick(BattleContext context, float deltaTime);
    }
}
