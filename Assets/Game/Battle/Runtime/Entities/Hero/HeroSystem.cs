using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Bullet;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.Hero
{
    /// <summary>
    /// 英雄系统：最小闭环下负责两件事：
    /// 1) 自动索敌并在冷却结束时发射子弹
    /// 2) 提供纯函数式的移动应用（由命令消费侧调用）
    /// </summary>
    public sealed class HeroSystem
    {
        /// <summary>
        /// 每逻辑帧更新：递减冷却并在条件允许时创建子弹实体。
        /// </summary>
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

        /// <summary>
        /// 将方向向量应用到英雄位置（方向会在方法内归一化）。
        /// </summary>
        public void ApplyMove(HeroEntity hero, Vector3 direction, float deltaTime)
        {
            Vector3 normalized = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.zero;
            hero.Position += normalized * hero.MoveSpeed * deltaTime;
        }

        /// <summary>
        /// 在攻击范围内选择距离最近的存活敌人作为目标。
        /// </summary>
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
