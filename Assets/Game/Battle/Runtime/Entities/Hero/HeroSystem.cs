using Game.Battle.Runtime.Core;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.Hero
{
    /// <summary>
    /// 英雄系统：当前只负责移动应用。
    /// 索敌已拆到 HeroTargetingService；发射已拆到 WeaponFireService；
    /// 状态已拆到 HeroStateController。
    /// </summary>
    public sealed class HeroSystem
    {
        /// <summary>每逻辑帧更新（当前由 BattleWorld 拆分驱动，此处预留扩展）。</summary>
        public void Tick(BattleContext context, float deltaTime)
        {
        }

        /// <summary>将方向向量应用到英雄位置。</summary>
        public void ApplyMove(HeroEntity hero, Vector3 direction, float deltaTime)
        {
            Vector3 normalized = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.zero;
            hero.Position += normalized * hero.MoveSpeed * deltaTime;
        }
    }
}
