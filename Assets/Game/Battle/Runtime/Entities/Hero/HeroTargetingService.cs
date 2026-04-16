using Game.Battle.Runtime.Core;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.Hero
{
    /// <summary>
    /// 英雄锁敌服务：从 HeroSystem 中拆出，使锁敌策略可替换。
    /// </summary>
    public sealed class HeroTargetingService
    {
        /// <summary>在攻击范围内选择距离最近的存活敌人 Id。</summary>
        public string? FindNearestAliveEnemy(BattleContext context, HeroEntity hero)
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
