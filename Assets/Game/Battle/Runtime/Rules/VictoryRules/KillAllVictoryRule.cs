using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Rules.VictoryRules
{
    /// <summary>
    /// 最小胜利规则：当关卡流程判定清场后，立即宣告胜利并结束战斗。
    /// <para>
    /// 命名说明：类名沿用迁移文档里的 <c>KillAllVictoryRule</c>，但当前实现绑定的是
    /// <see cref="Game.Battle.Runtime.Rules.StageHandlers.IStageHandler.IsStageCleared"/>（后续可替换为真正“击杀计数”规则）。
    /// </para>
    /// </summary>
    public sealed class KillAllVictoryRule : IVictoryRule
    {
        /// <inheritdoc />
        public bool IsBattleFinished { get; private set; }

        /// <inheritdoc />
        public bool IsVictory { get; private set; }

        /// <inheritdoc />
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
