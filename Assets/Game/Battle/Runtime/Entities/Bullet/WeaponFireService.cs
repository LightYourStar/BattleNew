using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Hero;

namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// 武器发射服务：把"发射时机判断"从 HeroSystem.Tick 中拆出，
    /// 使 HeroSystem 只关心状态和移动，由 WeaponFireService 负责生成子弹。
    /// </summary>
    public sealed class WeaponFireService
    {
        private readonly BulletFactory _factory;

        public WeaponFireService(BulletFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// 尝试为英雄发射子弹：硬直检查 + 冷却判断 + 目标选择 + 子弹生成。
        /// </summary>
        public void TryFire(BattleContext context, HeroEntity hero, string? targetId, float deltaTime)
        {
            // 硬直期间不能攻击
            if (hero.StunRemaining > 0f)
            {
                return;
            }

            hero.AttackCooldownRemaining -= deltaTime;
            if (hero.AttackCooldownRemaining > 0f || string.IsNullOrEmpty(targetId))
            {
                return;
            }

            BulletEntity bullet = _factory.CreateTracking(hero.Id, targetId, hero.Position);
            context.Registry.Bullets.Add(bullet);
            hero.AttackCooldownRemaining = hero.AttackCooldownSeconds;
        }
    }
}
