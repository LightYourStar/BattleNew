using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.AI;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// Handles bullet flight, hit detection and minimal damage application.
    /// </summary>
    public sealed class BulletSystem
    {
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
