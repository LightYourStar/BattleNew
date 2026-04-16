using UnityEngine;

namespace Game.Battle.Runtime.Entities.AI
{
    public sealed class AIEntity
    {
        public string Id { get; }

        public Vector3 Position { get; set; }

        public float MaxHp { get; }

        public float CurrentHp { get; private set; }

        public float MoveSpeed { get; set; } = 2f;

        public bool IsAlive => CurrentHp > 0f;

        public AIEntity(string id, Vector3 spawnPosition, float maxHp)
        {
            Id = id;
            Position = spawnPosition;
            MaxHp = maxHp;
            CurrentHp = maxHp;
        }

        public void ReceiveDamage(float damage)
        {
            CurrentHp -= damage;
            if (CurrentHp < 0f)
            {
                CurrentHp = 0f;
            }
        }
    }
}
