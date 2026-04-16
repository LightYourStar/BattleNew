using UnityEngine;

namespace Game.Battle.Runtime.Entities.Bullet
{
    public sealed class BulletEntity
    {
        public string Id { get; }

        public string OwnerId { get; }

        public string TargetId { get; }

        public Vector3 Position { get; set; }

        public float Speed { get; set; } = 12f;

        public float Damage { get; set; } = 10f;

        public float HitRadius { get; set; } = 0.25f;

        public bool IsActive { get; set; } = true;

        public BulletEntity(string id, string ownerId, string targetId, Vector3 spawnPosition)
        {
            Id = id;
            OwnerId = ownerId;
            TargetId = targetId;
            Position = spawnPosition;
        }
    }
}
