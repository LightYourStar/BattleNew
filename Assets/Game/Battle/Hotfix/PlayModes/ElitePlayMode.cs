using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Rules.PlayModes;

namespace Game.Battle.Hotfix.PlayModes
{
    /// <summary>
    /// 示例热更玩法：精英关模式，可在开始/结束时注入特殊规则。
    /// </summary>
    public sealed class ElitePlayMode : IPlayMode
    {
        public void OnBattleStart(BattleContext context)
        {
            // 可在此增加精英关特殊初始化（如 Buff 加成等）
        }

        public void OnBattleEnd(BattleContext context, bool isVictory)
        {
            // 可在此添加精英关特殊结算逻辑
        }
    }
}
