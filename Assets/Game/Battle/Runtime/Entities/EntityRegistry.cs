using System.Collections.Generic;
using Game.Battle.Runtime.Entities.AI;
using Game.Battle.Runtime.Entities.Bullet;
using Game.Battle.Runtime.Entities.Hero;

namespace Game.Battle.Runtime.Entities
{
    /// <summary>
    /// Unified runtime container for core battle entities.
    /// </summary>
    public sealed class EntityRegistry
    {
        public List<HeroEntity> Heroes { get; } = new();

        public List<AIEntity> Enemies { get; } = new();

        public List<BulletEntity> Bullets { get; } = new();
    }
}
