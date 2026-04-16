using UnityEngine;

namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// 抛物线飞行策略（预留骨架）：
    /// 实现思路是在生成时计算水平速度 + 初始垂直速度，每帧叠加重力加速度。
    /// </summary>
    public sealed class ParabolicMovement : IBulletMovement
    {
        private readonly float _gravity;
        private float _verticalVelocity;
        private readonly Vector3 _horizontalDirection;

        public ParabolicMovement(Vector3 horizontalDirection, float initialVerticalSpeed = 8f, float gravity = 18f)
        {
            _horizontalDirection = horizontalDirection.sqrMagnitude > 0f
                ? horizontalDirection.normalized
                : Vector3.forward;
            _verticalVelocity = initialVerticalSpeed;
            _gravity = gravity;
        }

        public void Tick(BulletEntity bullet, Vector3 targetPosition, float deltaTime)
        {
            _verticalVelocity -= _gravity * deltaTime;
            Vector3 velocity = _horizontalDirection * bullet.Speed + Vector3.up * _verticalVelocity;
            bullet.Position += velocity * deltaTime;
        }
    }
}
