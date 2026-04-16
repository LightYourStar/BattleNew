using UnityEngine;

namespace Game.Battle.Runtime.Entities.Hero
{
    public sealed class HeroEntity
    {
        public string Id { get; }

        public Vector3 Position { get; set; }

        public float MoveSpeed { get; set; } = 5f;

        public float AttackRange { get; set; } = 5f;

        public float AttackCooldownSeconds { get; set; } = 0.75f;

        public float AttackCooldownRemaining { get; set; }

        public HeroEntity(string id, Vector3 spawnPosition)
        {
            Id = id;
            Position = spawnPosition;
            AttackCooldownRemaining = 0f;
        }
    }
}
