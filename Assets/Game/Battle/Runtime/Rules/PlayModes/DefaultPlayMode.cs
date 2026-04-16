using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Rules.PlayModes
{
    /// <summary>
    /// Default ruleset hook for minimal PvE loop.
    /// </summary>
    public sealed class DefaultPlayMode : IPlayMode
    {
        public void OnBattleStart(BattleContext context)
        {
        }

        public void OnBattleEnd(BattleContext context, bool isVictory)
        {
        }
    }
}
