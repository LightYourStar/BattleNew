using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.AI;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// 命中判定器：把"距离判定 + 触发伤害"从 BulletSystem 中拆出。
    /// 后续可在这里插入闪避/格挡/Buff 修正等前置检查。
    /// </summary>
    public sealed class HitResolver
    {
        /// <summary>
        /// 判定子弹是否命中目标；如果命中，通过 DamageService 走统一伤害链路。
        /// 返回 true 表示已命中（子弹应被回收）。
        /// </summary>
        public bool TryResolveHit(BattleContext context, BulletEntity bullet, AIEntity target)
        {
            float distance = Vector3.Distance(bullet.Position, target.Position);
            if (distance > bullet.HitRadius)
            {
                return false;
            }

            context.DamageService.ApplyDamage(context, bullet.OwnerId, target.Id, bullet.Damage);
            return true;
        }
    }
}
