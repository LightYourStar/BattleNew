using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.AI;
using Game.Battle.Runtime.Entities.Element;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// 命中判定器：距离检查 → 触发伤害 → 触发受击反应，三步链路。
    /// <para>
    /// 职责边界：
    /// <list type="bullet">
    ///   <item>判断子弹是否与目标足够近（距离 ≤ HitRadius）。</item>
    ///   <item>命中后通过 DamageService 走统一伤害链路（含无敌/Buff/Trait 修正）。</item>
    ///   <item>命中后通过 HitReactionService 触发击退 + 硬直（独立于伤害计算）。</item>
    /// </list>
    /// </para>
    /// </summary>
    public sealed class HitResolver
    {
        /// <summary>子弹命中目标时施加给 AI 的默认击退力。</summary>
        private const float DefaultBulletKnockbackForce = 4f;

        /// <summary>子弹命中目标时施加给 AI 的默认硬直时长（秒）。</summary>
        private const float DefaultBulletStunDuration = 0.12f;

        /// <summary>
        /// 判定子弹是否命中目标；如果命中，依次触发伤害链 + 受击反应链。
        /// 返回 true 表示已命中（子弹应被回收）。
        /// </summary>
        public bool TryResolveHit(BattleContext context, BulletEntity bullet, AIEntity target)
        {
            float distance = Vector3.Distance(bullet.Position, target.Position);
            if (distance > bullet.HitRadius)
            {
                return false;
            }

            // ── 伤害链（含无敌检查 / Buff / Trait 修正）
            context.DamageService.ApplyDamage(context, bullet.OwnerId, target.Id, bullet.Damage);

            // ── 受击反应链（击退 + 硬直，独立于伤害数值）
            // 击退方向：子弹位置 → 目标位置（把目标从子弹来源处推开）
            Vector3 knockbackDir = target.Position - bullet.Position;
            HitReactionContext reactionCtx = new(bullet.OwnerId, target.Id, knockbackDir)
            {
                KnockbackForce = DefaultBulletKnockbackForce,
                StunDuration = DefaultBulletStunDuration,
                InvincibilityDuration = 0f  // AI 不需要无敌帧
            };
            context.HitReactionService.ProcessHit(context, reactionCtx);

            return true;
        }
    }
}
