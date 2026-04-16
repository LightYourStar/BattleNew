using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Hero;
using UnityEngine;

namespace Game.Battle.Runtime.Bootstrap
{
    /// <summary>
    /// Entry point that owns world lifecycle only.
    /// </summary>
    public sealed class BattleBootstrap
    {
        public BattleWorld? World { get; private set; }

        public void EnterBattle(BattleWorld? world = null)
        {
            World = world ?? new BattleWorld();
            SeedMinimalLoop(World.Context);
            World.Start();
        }

        public void Update(float deltaTime)
        {
            World?.Update(deltaTime);
        }

        public void ExitBattle()
        {
            if (World == null)
            {
                return;
            }

            World.Stop();
            World = null;
        }

        private static void SeedMinimalLoop(BattleContext context)
        {
            context.Registry.Heroes.Add(new HeroEntity("hero_1", Vector3.zero));
        }
    }
}
