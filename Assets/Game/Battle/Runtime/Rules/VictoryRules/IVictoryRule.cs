using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Rules.VictoryRules
{
    /// <summary>
    /// 胜负规则：把“何时结束战斗、胜负结果是什么”从玩法/关卡细节中独立出来。
    /// </summary>
    public interface IVictoryRule
    {
        /// <summary>战斗是否已结束（结束后 BattleWorld 会停止循环）。</summary>
        bool IsBattleFinished { get; }

        /// <summary>若战斗结束，本局是否胜利。</summary>
        bool IsVictory { get; }

        /// <summary>每逻辑帧更新：用于检查胜负条件。</summary>
        void Tick(BattleContext context, float deltaTime);
    }
}
