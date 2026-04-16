using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Rules.PlayModes
{
    /// <summary>
    /// 默认玩法：最小闭环下提供空实现，保证 BattleWorld 生命周期钩子可运行。
    /// <para>
    /// 后续可在这里接入：体力扣除、模式参数、UI 打开、统计上报等玩法级逻辑。
    /// </para>
    /// </summary>
    public sealed class DefaultPlayMode : IPlayMode
    {
        /// <inheritdoc />
        public void OnBattleStart(BattleContext context)
        {
        }

        /// <inheritdoc />
        public void OnBattleEnd(BattleContext context, bool isVictory)
        {
        }
    }
}
