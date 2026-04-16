using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Hero;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.AI
{
    /// <summary>
    /// AI 系统：状态判定 + 追击移动 + 攻击，并对英雄造成伤害。
    /// </summary>
    public sealed class AISystem
    {
        /// <summary>每逻辑帧更新所有敌人。</summary>
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

                switch (enemy.State)
                {
                    case AIState.Pursue:
                        TickPursue(context, enemy, deltaTime);
                        break;
                    case AIState.Attack:
                        TickAttack(context, enemy, deltaTime);
                        break;
                }
            }
        }

        private static void TickPursue(BattleContext context, AIEntity enemy, float deltaTime)
        {
            if (context.Registry.Heroes.Count == 0)
            {
                return;
            }

            HeroEntity hero = context.Registry.Heroes[0];
            Vector3 direction = hero.Position - enemy.Position;
            if (direction.sqrMagnitude < 0.01f)
            {
                return;
            }

            enemy.Position += direction.normalized * enemy.MoveSpeed * deltaTime;
        }

        private static void TickAttack(BattleContext context, AIEntity enemy, float deltaTime)
        {
            enemy.AttackCooldownRemaining -= deltaTime;
            if (enemy.AttackCooldownRemaining > 0f)
            {
                return;
            }

            if (context.Registry.Heroes.Count == 0)
            {
                return;
            }

            HeroEntity target = context.Registry.Heroes[0];
            context.DamageService.ApplyDamage(context, enemy.Id, target.Id, enemy.AttackDamage);
            enemy.AttackCooldownRemaining = enemy.AttackCooldown;
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

            if (distance <= enemy.AttackRange)
            {
                return AIState.Attack;
            }

            if (distance <= enemy.SightRange)
            {
                return AIState.Pursue;
            }

            return AIState.Idle;
        }
    }
}
