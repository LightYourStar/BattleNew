using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Bullet;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.Hero
{
    /// <summary>
    /// Handles hero movement/attack cadence in a minimal deterministic way.
    /// </summary>
    public sealed class HeroSystem
    {
        public void Tick(BattleContext context, float deltaTime)
        {
            for (int i = 0; i < context.Registry.Heroes.Count; i++)
            {
                HeroEntity hero = context.Registry.Heroes[i];
                hero.AttackCooldownRemaining -= deltaTime;
                if (hero.AttackCooldownRemaining > 0f)
                {
                    continue;
                }

                string? targetId = FindNearestTargetId(context, hero);
                if (string.IsNullOrEmpty(targetId))
                {
                    continue;
                }

                BulletEntity bullet = new(
                    id: $"bullet_{context.Time.Frame}_{i}",
                    ownerId: hero.Id,
                    targetId: targetId,
                    spawnPosition: hero.Position);

                context.Registry.Bullets.Add(bullet);
                hero.AttackCooldownRemaining = hero.AttackCooldownSeconds;
            }
        }

        public void ApplyMove(HeroEntity hero, Vector3 direction, float deltaTime)
        {
            Vector3 normalized = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.zero;
            hero.Position += normalized * hero.MoveSpeed * deltaTime;
        }

        private static string? FindNearestTargetId(BattleContext context, HeroEntity hero)
        {
            float bestDistance = float.MaxValue;
            string? bestTarget = null;

            for (int i = 0; i < context.Registry.Enemies.Count; i++)
            {
                var enemy = context.Registry.Enemies[i];
                if (!enemy.IsAlive)
                {
                    continue;
                }

                float distance = Vector3.Distance(hero.Position, enemy.Position);
                if (distance > hero.AttackRange || distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                bestTarget = enemy.Id;
            }

            return bestTarget;
        }
    }
}
