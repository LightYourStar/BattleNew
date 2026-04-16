using UnityEngine;

namespace Game.Battle.Runtime.Entities.Hero
{
    /// <summary>
    /// 英雄移动控制器：把"位置推进"和"朝向更新"集中管理。
    /// <para>
    /// 职责边界（只做以下两件事，其余都不管）：
    /// <list type="bullet">
    ///   <item>根据输入方向推进英雄逻辑位置。</item>
    ///   <item>维护英雄朝向（<see cref="HeroEntity.FacingDirection"/>）。</item>
    /// </list>
    /// </para>
    /// <para>
    /// 设计思路：
    /// - 移动时：朝移动方向。
    /// - 有锁定目标且不在移动：拉向目标方向（持续瞄准）。
    /// - 两者都没有：保持当前朝向，不强制清零。
    /// </para>
    /// </summary>
    public sealed class HeroMovementController
    {
        /// <summary>
        /// 推进英雄位置并更新朝向（由移动命令触发）。
        /// <para>在 BattleWorld 消费 MoveCommand 时调用。</para>
        /// </summary>
        /// <param name="hero">目标英雄实体。</param>
        /// <param name="direction">输入方向（可不归一化，内部会归一化）。</param>
        /// <param name="deltaTime">固定逻辑步长。</param>
        public void ApplyMove(HeroEntity hero, Vector3 direction, float deltaTime)
        {
            if (direction.sqrMagnitude <= 0f)
            {
                return;
            }

            Vector3 normalized = direction.normalized;
            hero.Position += normalized * hero.MoveSpeed * deltaTime;

            // 移动时朝移动方向
            hero.FacingDirection = normalized;
        }

        /// <summary>
        /// 把英雄朝向拉向目标（有锁定目标且当前未在移动时每帧调用）。
        /// <para>
        /// 这保证了"面向与攻击目标同步"——即使站桩射击，人物也面朝敌人。
        /// </para>
        /// </summary>
        /// <param name="hero">目标英雄实体。</param>
        /// <param name="targetPosition">目标位置。</param>
        public void ApplyFaceTarget(HeroEntity hero, Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - hero.Position;
            direction.y = 0f; // 忽略高度差，保持水平面朝向

            if (direction.sqrMagnitude <= 0f)
            {
                return;
            }

            hero.FacingDirection = direction.normalized;
        }
    }
}
