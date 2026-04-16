using UnityEngine;

namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// 子弹实体：最小闭环下用“追踪目标 + 距离命中”模拟飞行与命中。
    /// </summary>
    public sealed class BulletEntity
    {
        /// <summary>子弹 Id（用于可视化映射与调试）。</summary>
        public string Id { get; }

        /// <summary>发射者实体 Id（通常是英雄）。</summary>
        public string OwnerId { get; }

        /// <summary>目标实体 Id（通常是敌人）。</summary>
        public string TargetId { get; }

        /// <summary>逻辑位置（世界空间）。</summary>
        public Vector3 Position { get; set; }

        /// <summary>飞行速度（单位：世界坐标/秒）。</summary>
        public float Speed { get; set; } = 12f;

        /// <summary>命中伤害。</summary>
        public float Damage { get; set; } = 10f;

        /// <summary>命中半径：与目标距离小于等于该值判定为命中。</summary>
        public float HitRadius { get; set; } = 0.25f;

        /// <summary>是否仍活跃（命中/目标失效后会置 false 并回收）。</summary>
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
