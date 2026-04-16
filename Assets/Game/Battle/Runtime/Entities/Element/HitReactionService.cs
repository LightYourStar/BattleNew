using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.AI;
using Game.Battle.Runtime.Entities.Hero;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.Element
{
    /// <summary>
    /// 受击反应服务：击退、受击硬直、无敌帧的完整实现，与伤害数值计算完全解耦。
    /// <para>
    /// 三大能力独立于 DamageService：
    /// <list type="bullet">
    ///   <item><b>击退</b>：命中时给目标施加 KnockbackVelocity，Tick 每帧推进位移并衰减。</item>
    ///   <item><b>受击硬直</b>：命中时设置 StunRemaining，期间 Hero 不能移动/攻击，AI 跳过状态机。</item>
    ///   <item><b>无敌帧</b>（仅英雄）：命中后设置 InvincibleRemaining，DamageService 前置检查会跳过该时段内的后续伤害。</item>
    /// </list>
    /// </para>
    /// </summary>
    public sealed class HitReactionService
    {
        /// <summary>击退速度的每帧阻力系数（越大衰减越快）。</summary>
        private const float KnockbackDrag = 8f;

        // ─── 主要接口 ─────────────────────────────────────────────────────────

        /// <summary>
        /// 判断目标是否处于无敌状态（供 DamageService 前置检查）。
        /// 只对英雄生效；AI 不设计无敌帧。
        /// </summary>
        public bool IsInvincible(BattleContext context, string targetId)
        {
            HeroEntity? hero = context.Registry.FindHero(targetId);
            return hero != null && hero.InvincibleRemaining > 0f;
        }

        /// <summary>
        /// 处理一次命中的受击反应：击退 + 硬直 + 无敌帧。
        /// <para>
        /// 在 <see cref="DamageService.ApplyDamage"/> 确认命中并扣血后调用，
        /// 或由 AISystem 的近战攻击命中后直接调用。
        /// </para>
        /// </summary>
        public void ProcessHit(BattleContext context, HitReactionContext reactionCtx)
        {
            // 尝试对 AI 敌人施加反应
            AIEntity? enemy = context.Registry.FindAI(reactionCtx.TargetId);
            if (enemy != null && enemy.IsAlive)
            {
                ApplyReactionToAI(enemy, reactionCtx);
                context.DebugTraceService.TraceStateChange(
                    enemy.Id, "Normal",
                    $"HitReaction(stun={reactionCtx.StunDuration:F2}s,kb={reactionCtx.KnockbackForce:F1})");
                return;
            }

            // 尝试对英雄施加反应
            HeroEntity? hero = context.Registry.FindHero(reactionCtx.TargetId);
            if (hero != null)
            {
                ApplyReactionToHero(hero, reactionCtx);
                context.DebugTraceService.TraceStateChange(
                    hero.Id, "Normal",
                    $"HitReaction(stun={reactionCtx.StunDuration:F2}s,invnc={reactionCtx.InvincibilityDuration:F2}s)");
            }
        }

        /// <summary>
        /// 每逻辑帧更新所有实体的受击反应状态：
        /// 推进击退位移、衰减击退速度、递减硬直 / 无敌计时器。
        /// 由 BattleWorld 在每帧 Tick 中调用（放在 Hero/AI 系统更新之前）。
        /// </summary>
        public void Tick(BattleContext context, float deltaTime)
        {
            TickHeroes(context, deltaTime);
            TickEnemies(context, deltaTime);
        }

        // ─── 私有：施加反应 ───────────────────────────────────────────────────

        private static void ApplyReactionToAI(AIEntity enemy, HitReactionContext ctx)
        {
            // 击退（叠加：快速连击可累积）
            if (ctx.KnockbackForce > 0f)
            {
                enemy.KnockbackVelocity += ctx.KnockbackDirection * ctx.KnockbackForce;
            }

            // 硬直（取最大值：已有更长硬直时不缩短）
            if (ctx.StunDuration > enemy.StunRemaining)
            {
                enemy.StunRemaining = ctx.StunDuration;
            }
        }

        private static void ApplyReactionToHero(HeroEntity hero, HitReactionContext ctx)
        {
            // 击退
            if (ctx.KnockbackForce > 0f)
            {
                hero.KnockbackVelocity += ctx.KnockbackDirection * ctx.KnockbackForce;
            }

            // 硬直
            if (ctx.StunDuration > hero.StunRemaining)
            {
                hero.StunRemaining = ctx.StunDuration;
            }

            // 无敌帧
            if (ctx.InvincibilityDuration > hero.InvincibleRemaining)
            {
                hero.InvincibleRemaining = ctx.InvincibilityDuration;
            }
        }

        // ─── 私有：每帧 Tick ──────────────────────────────────────────────────

        private static void TickHeroes(BattleContext context, float deltaTime)
        {
            for (int i = 0; i < context.Registry.Heroes.Count; i++)
            {
                HeroEntity hero = context.Registry.Heroes[i];

                if (hero.StunRemaining > 0f)
                {
                    hero.StunRemaining = Mathf.Max(0f, hero.StunRemaining - deltaTime);
                }

                if (hero.InvincibleRemaining > 0f)
                {
                    hero.InvincibleRemaining = Mathf.Max(0f, hero.InvincibleRemaining - deltaTime);
                }

                if (hero.KnockbackVelocity.sqrMagnitude > 0.001f)
                {
                    hero.Position += hero.KnockbackVelocity * deltaTime;
                    Vector3 vel = hero.KnockbackVelocity * Mathf.Max(0f, 1f - KnockbackDrag * deltaTime);
                    hero.KnockbackVelocity = vel.sqrMagnitude > 0.001f ? vel : Vector3.zero;
                }
            }
        }

        private static void TickEnemies(BattleContext context, float deltaTime)
        {
            for (int i = 0; i < context.Registry.Enemies.Count; i++)
            {
                AIEntity enemy = context.Registry.Enemies[i];
                if (!enemy.IsAlive)
                {
                    continue;
                }

                if (enemy.StunRemaining > 0f)
                {
                    enemy.StunRemaining = Mathf.Max(0f, enemy.StunRemaining - deltaTime);
                }

                if (enemy.KnockbackVelocity.sqrMagnitude > 0.001f)
                {
                    enemy.Position += enemy.KnockbackVelocity * deltaTime;
                    Vector3 vel = enemy.KnockbackVelocity * Mathf.Max(0f, 1f - KnockbackDrag * deltaTime);
                    enemy.KnockbackVelocity = vel.sqrMagnitude > 0.001f ? vel : Vector3.zero;
                }
            }
        }
    }
}
