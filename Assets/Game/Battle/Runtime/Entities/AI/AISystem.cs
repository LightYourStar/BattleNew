using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Hero;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.AI
{
    /// <summary>
    /// AI 系统：最小闭环版本包含“状态判定 + 追击移动”。
    /// <para>
    /// 说明：
    /// - 当前默认只追逐 <c>Heroes[0]</c>，用于单英雄验证；多英雄时应引入锁敌服务或黑板数据。
    /// - 状态切换会触发 <see cref="Game.Battle.Runtime.Services.DebugTrace.IDebugTraceService.TraceStateChange"/> 以便观测。
    /// </para>
    /// </summary>
    public sealed class AISystem
    {
        /// <summary>每逻辑帧更新：先更新状态，再执行 Pursue 下的位移。</summary>
        public void Tick(BattleContext context, float deltaTime)
        {
            for (int i = 0; i < context.Registry.Enemies.Count; i++)
            {
                AIEntity enemy = context.Registry.Enemies[i];
                AIState nextState = ResolveState(context, enemy);
                if (enemy.State != nextState)
                {
                    context.DebugTraceService.TraceStateChange(enemy.Id, enemy.State.ToString(), nextState.ToString());
                    enemy.SetState(nextState);
                }

                if (enemy.State != AIState.Pursue || context.Registry.Heroes.Count == 0)
                {
                    continue;
                }

                HeroEntity hero = context.Registry.Heroes[0];
                if (!enemy.IsAlive)
                {
                    continue;
                }

                Vector3 direction = hero.Position - enemy.Position;
                if (direction.sqrMagnitude < 0.01f)
                {
                    continue;
                }

                enemy.Position += direction.normalized * enemy.MoveSpeed * deltaTime;
            }
        }

        /// <summary>
        /// 计算下一状态：死亡优先；无英雄则 Idle；进入视野则 Pursue。
        /// </summary>
        private static AIState ResolveState(BattleContext context, AIEntity enemy)
        {
            if (!enemy.IsAlive)
            {
                return AIState.Dead;
            }

            if (context.Registry.Heroes.Count == 0)
            {
                return AIState.Idle;
            }

            HeroEntity hero = context.Registry.Heroes[0];
            float distance = Vector3.Distance(enemy.Position, hero.Position);
            if (distance <= enemy.SightRange)
            {
                return AIState.Pursue;
            }

            return AIState.Idle;
        }
    }
}
