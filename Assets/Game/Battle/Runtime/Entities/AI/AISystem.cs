using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Hero;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.AI
{
    /// <summary>
    /// Runs minimal AI pursue behavior for the first battle slice.
    /// </summary>
    public sealed class AISystem
    {
        public void Tick(BattleContext context, float deltaTime)
        {
            if (context.Registry.Heroes.Count == 0)
            {
                return;
            }

            HeroEntity hero = context.Registry.Heroes[0];
            for (int i = 0; i < context.Registry.Enemies.Count; i++)
            {
                AIEntity enemy = context.Registry.Enemies[i];
                if (!enemy.IsAlive)
                {
                    continue;
                }

                Vector3 direction = hero.Position - enemy.Position;
                if (direction.sqrMagnitude < 0.01f)
                {
                    continue;
                }

                enemy.Position += direction.normalized * enemy.MoveSpeed * deltaTime;
            }
        }
    }
}
