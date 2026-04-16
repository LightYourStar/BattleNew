using UnityEngine;

namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// 直线飞行策略：在生成时锁定方向，之后每帧沿固定方向匀速移动，不跟随目标。
    /// </summary>
    public sealed class LinearMovement : IBulletMovement
    {
        /// <summary>已归一化的飞行方向（生成时锁定）。</summary>
        private readonly Vector3 _direction;

        public LinearMovement(Vector3 direction)
        {
            _direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
        }

        public void Tick(BulletEntity bullet, Vector3 targetPosition, float deltaTime)
        {
            bullet.Position += _direction * bullet.Speed * deltaTime;
        }
    }
}
