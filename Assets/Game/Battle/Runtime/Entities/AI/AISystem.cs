using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Hero;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.AI
{
    /// <summary>
    /// Runs minimal AI pursue behavior for the first battle slice.
    /// </summary>
    public sealed class AISystem
    {
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
