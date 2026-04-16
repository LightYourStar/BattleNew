using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.AI;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// 子弹系统：更新子弹位置、处理命中、触发最小伤害与回收。
    /// <para>
    /// 注意：这是教学/验证向的最小实现，不是最终性能形态。
    /// 后续切片会拆出 HitResolver、伤害请求队列、碰撞体配置等。
    /// </para>
    /// </summary>
    public sealed class BulletSystem
    {
        /// <summary>
        /// 逆序遍历便于安全 RemoveAt；每颗子弹在本帧最多结算一次命中。
        /// </summary>
        public void Tick(BattleContext context, float deltaTime)
        {
            for (int i = context.Registry.Bullets.Count - 1; i >= 0; i--)
            {
                BulletEntity bullet = context.Registry.Bullets[i];
                if (!bullet.IsActive)
                {
                    context.Registry.Bullets.RemoveAt(i);
                    continue;
                }

                AIEntity? target = FindTarget(context, bullet.TargetId);
                if (target == null || !target.IsAlive)
                {
                    bullet.IsActive = false;
                    context.Registry.Bullets.RemoveAt(i);
                    continue;
                }

                Vector3 direction = target.Position - bullet.Position;
                if (direction.sqrMagnitude > 0f)
                {
                    bullet.Position += direction.normalized * bullet.Speed * deltaTime;
                }

                float distance = Vector3.Distance(bullet.Position, target.Position);
                if (distance > bullet.HitRadius)
                {
                    continue;
                }

                target.ReceiveDamage(bullet.Damage);
                context.DebugTraceService.TraceDamage(bullet.OwnerId, target.Id, bullet.Damage);
                bullet.IsActive = false;
                context.Registry.Bullets.RemoveAt(i);
            }
        }

        /// <summary>在敌人列表中按 Id 查找目标实体。</summary>
        private static AIEntity? FindTarget(BattleContext context, string targetId)
        {
            for (int i = 0; i < context.Registry.Enemies.Count; i++)
            {
                AIEntity enemy = context.Registry.Enemies[i];
                if (enemy.Id == targetId)
                {
                    return enemy;
                }
            }

            return null;
        }
    }
}
