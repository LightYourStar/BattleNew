using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Rules.StageHandlers
{
    public interface IStageHandler
    {
        bool IsStageCleared { get; }

        void Tick(BattleContext context, float deltaTime);
    }
}
