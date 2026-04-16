using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Element;
using Game.Battle.Runtime.Entities.Hero;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.AI
{
    /// <summary>
    /// AI 系统：状态判定 + 追击移动 + 攻击。
    /// <para>
    /// 每帧执行顺序（per enemy）：
    /// 1. 硬直检查 → 硬直中跳过状态机更新与行为执行。
    /// 2. 解算下一状态（ResolveState）→ 若状态改变则追踪日志。
    /// 3. 按当前状态执行行为（Pursue 追击 / Attack 攻击 + 触发英雄受击反应）。
    /// </para>
    /// </summary>
    public sealed class AISystem
    {
        /// <summary>AI 近战攻击英雄时施加的默认击退力。</summary>
        private const float MeleeKnockbackForce = 2.5f;

        /// <summary>AI 近战攻击英雄时施加的默认硬直时长（秒）。</summary>
        private const float MeleeStunDuration = 0.15f;

        /// <summary>AI 近战攻击英雄后，英雄获得的无敌时长（秒）。</summary>
        private const float MeleeInvincibilityDuration = 0.5f;

        /// <summary>每逻辑帧更新所有敌人。</summary>
        public void Tick(BattleContext context, float deltaTime)
        {
            for (int i = 0; i < context.Registry.Enemies.Count; i++)
            {
                AIEntity enemy = context.Registry.Enemies[i];

                // 硬直期间跳过一切行为（位移与硬直倒计时由 HitReactionService.Tick 处理）
                if (enemy.StunRemaining > 0f)
                {
                    continue;
                }

                // 状态解算 + 日志
                AIState nextState = ResolveState(context, enemy);
                if (enemy.State != nextState)
                {
                    context.DebugTraceService.TraceStateChange(
                        enemy.Id, enemy.State.ToString(), nextState.ToString());
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

        // ─── 行为 ─────────────────────────────────────────────────────────────

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

            // 伤害链（含无敌检查）
            context.DamageService.ApplyDamage(context, enemy.Id, target.Id, enemy.AttackDamage);

            // 受击反应链（击退英雄 + 英雄获得无敌帧）
            // 击退方向：敌人位置 → 英雄位置（把英雄从敌人处推开）
            Vector3 knockbackDir = target.Position - enemy.Position;
            HitReactionContext reactionCtx = new(enemy.Id, target.Id, knockbackDir)
            {
                KnockbackForce = MeleeKnockbackForce,
                StunDuration = MeleeStunDuration,
                InvincibilityDuration = MeleeInvincibilityDuration
            };
            context.HitReactionService.ProcessHit(context, reactionCtx);

            enemy.AttackCooldownRemaining = enemy.AttackCooldown;
        }

        // ─── 状态解算 ─────────────────────────────────────────────────────────

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
