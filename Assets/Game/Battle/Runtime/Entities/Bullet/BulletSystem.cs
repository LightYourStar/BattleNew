using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.AI;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// 子弹系统：每逻辑帧驱动所有活跃子弹的飞行与命中判定。
    /// <para>
    /// 架构职责：
    /// <list type="bullet">
    ///   <item>遍历 Registry.Bullets，为每颗子弹调用其 <see cref="IBulletMovement.Tick"/> 推进位置。</item>
    ///   <item>推进后调用 <see cref="HitResolver.TryResolveHit"/> 判断是否命中目标。</item>
    ///   <item>命中或目标失效时回收子弹（IsActive=false，从列表移除）。</item>
    /// </list>
    /// BulletSystem 不关心"子弹怎么飞"——飞行细节由 IBulletMovement 实现类封装。
    /// BulletSystem 不关心"命中后做什么"——伤害由 HitResolver → DamageService 处理。
    /// </para>
    /// </summary>
    public sealed class BulletSystem
    {
        /// <summary>
        /// 每逻辑帧：驱动飞行策略 → 命中判定 → 回收。
        /// </summary>
        public void Tick(BattleContext context, float deltaTime)
        {
            var bullets = context.Registry.Bullets;

            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                BulletEntity bullet = bullets[i];

                // 查找目标；目标已死则直接回收
                AIEntity? target = context.Registry.FindAI(bullet.TargetId);
                if (target == null || !target.IsAlive)
                {
                    bullet.IsActive = false;
                    bullets.RemoveAt(i);
                    continue;
                }

                // 委托飞行策略推进位置（Strategy 模式：不同子弹类型行为不同）
                bullet.Movement.Tick(bullet, target.Position, deltaTime);

                // 命中判定：由 HitResolver 统一处理距离检查 + 伤害链
                if (context.HitResolver.TryResolveHit(context, bullet, target))
                {
                    bullet.IsActive = false;
                    bullets.RemoveAt(i);
                }
            }
        }
    }
}
