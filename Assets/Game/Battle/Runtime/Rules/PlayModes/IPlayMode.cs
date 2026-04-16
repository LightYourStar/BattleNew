using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Rules.PlayModes
{
    /// <summary>
    /// 玩法模式：表达“这一局战斗属于哪种玩法”的生命周期与全局规则钩子。
    /// <para>
    /// 边界：不要在这里写关卡波次细节（那属于 <c>StageHandler</c>），也不要写胜负条件（那属于 <c>VictoryRule</c>）。
    /// </para>
    /// </summary>
    public interface IPlayMode
    {
        /// <summary>战斗开始：可做玩法级初始化（当前默认实现为空）。</summary>
        void OnBattleStart(BattleContext context);

        /// <summary>战斗结束：可做结算入口触发（当前默认实现为空）。</summary>
        void OnBattleEnd(BattleContext context, bool isVictory);
    }
}
