using UnityEngine;

namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// 子弹工厂：集中构建不同飞行类型的子弹，便于后续接入配置表或对象池。
    /// <para>
    /// 扩展方式：新增一种子弹时，在这里添加对应的 Create 重载或工厂方法，
    /// 并传入相应的 IBulletMovement 实现，不需要改 BulletSystem 或 BattleWorld。
    /// </para>
    /// </summary>
    public sealed class BulletFactory
    {
        private int _nextId;

        /// <summary>
        /// 创建追踪弹（默认类型：每帧朝目标当前位置飞行）。
        /// </summary>
        public BulletEntity CreateTracking(string ownerId, string targetId,
            Vector3 spawnPosition, float damage = 10f, float speed = 12f)
        {
            string id = $"bullet_{_nextId++}";
            return new BulletEntity(id, ownerId, targetId, spawnPosition,
                new TrackingMovement())
            {
                Damage = damage,
                Speed = speed
            };
        }

        /// <summary>
        /// 创建直线弹（发射时锁定方向，之后匀速飞行，不跟随目标）。
        /// </summary>
        public BulletEntity CreateLinear(string ownerId, string targetId,
            Vector3 spawnPosition, Vector3 direction,
            float damage = 10f, float speed = 15f)
        {
            string id = $"bullet_{_nextId++}";
            return new BulletEntity(id, ownerId, targetId, spawnPosition,
                new LinearMovement(direction))
            {
                Damage = damage,
                Speed = speed
            };
        }

        /// <summary>
        /// 创建抛物线弹（炮弹、投掷物等）。
        /// </summary>
        public BulletEntity CreateParabolic(string ownerId, string targetId,
            Vector3 spawnPosition, Vector3 horizontalDirection,
            float damage = 20f, float speed = 10f)
        {
            string id = $"bullet_{_nextId++}";
            return new BulletEntity(id, ownerId, targetId, spawnPosition,
                new ParabolicMovement(horizontalDirection))
            {
                Damage = damage,
                Speed = speed
            };
        }
    }
}
