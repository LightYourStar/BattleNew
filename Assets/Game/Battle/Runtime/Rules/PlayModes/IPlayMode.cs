using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Rules.PlayModes
{
    public interface IPlayMode
    {
        void OnBattleStart(BattleContext context);

        void OnBattleEnd(BattleContext context, bool isVictory);
    }
}
