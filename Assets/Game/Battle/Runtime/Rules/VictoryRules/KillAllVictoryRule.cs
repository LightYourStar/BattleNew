using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Rules.VictoryRules
{
    /// <summary>
    /// 胜负规则：击杀所有敌人则胜利；任一英雄阵亡则战败。
    /// <para>
    /// 优先级：战败检测优先于胜利检测——若英雄在清场同帧阵亡，判定为战败。
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
            if (IsBattleFinished)
            {
                return;
            }

            // 战败检测（优先）：任意英雄 HP 归零
            for (int i = 0; i < context.Registry.Heroes.Count; i++)
            {
                if (!context.Registry.Heroes[i].IsAlive)
                {
                    IsBattleFinished = true;
                    IsVictory = false;
                    context.DebugTraceService.TraceStateChange("Battle", "Running", "Defeat");
                    return;
                }
            }

            // 胜利检测：关卡流程已清场
            if (context.StageHandler.IsStageCleared)
            {
                IsBattleFinished = true;
                IsVictory = true;
                context.DebugTraceService.TraceStateChange("Battle", "Running", "Victory");
            }
        }
    }
}
