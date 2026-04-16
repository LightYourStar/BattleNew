using UnityEngine;

namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// 子弹飞行策略接口：把"怎么飞"从 BulletSystem 中解耦。
    /// <para>
    /// 扩展方式：新增一种飞行类型时，只需新增一个实现类并在 BulletFactory 里注册，
    /// 不需要改 BulletSystem 或 BattleWorld。
    /// </para>
    /// </summary>
    public interface IBulletMovement
    {
        /// <summary>
        /// 每逻辑帧推进子弹位置。
        /// </summary>
        /// <param name="bullet">当前子弹实体。</param>
        /// <param name="targetPosition">目标当前位置（追踪弹用到；直线弹可忽略）。</param>
        /// <param name="deltaTime">固定逻辑步长。</param>
        void Tick(BulletEntity bullet, Vector3 targetPosition, float deltaTime);
    }
}
