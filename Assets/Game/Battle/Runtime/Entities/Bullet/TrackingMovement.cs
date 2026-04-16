using UnityEngine;

namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// 追踪弹飞行策略：每帧朝目标当前位置方向移动，实现追踪效果。
    /// </summary>
    public sealed class TrackingMovement : IBulletMovement
    {
        public void Tick(BulletEntity bullet, Vector3 targetPosition, float deltaTime)
        {
            Vector3 direction = targetPosition - bullet.Position;
            if (direction.sqrMagnitude <= 0f)
            {
                return;
            }

            bullet.Position += direction.normalized * bullet.Speed * deltaTime;
        }
    }
}
