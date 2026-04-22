using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Hero;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// 武器发射服务：把"发射时机 + 枪口计算 + 子弹生成"从 HeroSystem 中完全拆出。
    /// <para>
    /// 职责链：
    /// <list type="number">
    ///   <item>硬直检查：硬直期间拒绝发射。</item>
    ///   <item>武器检查：英雄未装备武器时跳过。</item>
    ///   <item>冷却计时：每帧递减 <see cref="WeaponRuntime.CooldownRemaining"/>。</item>
    ///   <item>目标检查：无锁定目标时跳过。</item>
    ///   <item>枪口迭代：遍历 <see cref="WeaponDef.Sockets"/>，每个枪口各产生一颗子弹。</item>
    ///   <item>弹道路由：按 <see cref="WeaponDef.BulletType"/> 调用对应的 <see cref="BulletFactory"/> 方法。</item>
    /// </list>
    /// </para>
    /// </summary>
    public sealed class WeaponFireService
    {
        private readonly BulletFactory _factory;

        public WeaponFireService(BulletFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// 每逻辑帧调用：尝试为英雄发射子弹。
        /// </summary>
        public void TryFire(BattleContext context, HeroEntity hero, string? targetId, float deltaTime)
        {
            // 受击硬直期间不能攻击
            if (hero.StunRemaining > 0f)
            {
                return;
            }

            // 未装备武器
            WeaponRuntime? weapon = hero.CurrentWeapon;
            if (weapon == null)
            {
                return;
            }

            // 冷却计时（无论是否有目标都递减，保持时序一致）
            weapon.CooldownRemaining -= deltaTime;

            if (weapon.CooldownRemaining > 0f || string.IsNullOrEmpty(targetId))
            {
                return;
            }

            // 目标检查：目标实体是否存活
            var target = context.Registry.FindAI(targetId);
            if (target == null || !target.IsAlive)
            {
                return;
            }

            WeaponDef def = weapon.Def;

            // 旋转四元数：将局部坐标旋转到英雄当前朝向
            Quaternion facingRot = hero.FacingDirection != Vector3.zero
                ? Quaternion.LookRotation(hero.FacingDirection, Vector3.up)
                : Quaternion.identity;

            // 枪口迭代：每个 Socket 产生一颗子弹
            foreach (FireSocket socket in def.Sockets)
            {
                Vector3 worldMuzzle = hero.Position + facingRot * socket.LocalOffset;
                Vector3 worldForward = facingRot * socket.LocalForward;

                BulletEntity bullet = def.BulletType switch
                {
                    BulletType.Tracking =>
                        _factory.CreateTracking(hero.Id, targetId, worldMuzzle, def.Damage, def.BulletSpeed),
                    BulletType.Linear =>
                        _factory.CreateLinear(hero.Id, targetId, worldMuzzle, worldForward, def.Damage, def.BulletSpeed),
                    BulletType.Parabolic =>
                        _factory.CreateParabolic(hero.Id, targetId, worldMuzzle, worldForward, def.Damage, def.BulletSpeed),
                    _ =>
                        _factory.CreateTracking(hero.Id, targetId, worldMuzzle, def.Damage, def.BulletSpeed),
                };

                context.Registry.Bullets.Add(bullet);
            }

            // 重置冷却
            weapon.CooldownRemaining = weapon.CooldownSeconds;
        }
    }
}
