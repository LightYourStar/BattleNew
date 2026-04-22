using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Bullet;
using Game.Battle.Runtime.Entities.Hero;

namespace Game.Battle.Hotfix.Bootstrap
{
    /// <summary>
    /// 热更定义注册器：注册 HeroDef 和 WeaponDef 到对应注册表。
    /// <para>
    /// 设计原则：Runtime 层只有空注册表，所有具体数值在此注册，热更时替换此文件即可。
    /// </para>
    /// </summary>
    public static class HotfixDefRegistrar
    {
        /// <summary>注册所有英雄原型定义。</summary>
        public static void RegisterHeroes(BattleContext ctx)
        {
            // 战士：高 HP，近战攻击范围，携带战士词条池，默认武器为单手剑
            ctx.HeroDefRegistry.Register(new HeroDef("hero_warrior")
            {
                MaxHp = 120f,
                MoveSpeed = 4.5f,
                AttackRange = 4f,
                DefaultWeaponId = "weapon_sword",
                TraitPoolIds = new[] { "pool_warrior" },
            });

            // 射手：低 HP，远程攻击范围，携带弓词条池，默认武器为长弓
            ctx.HeroDefRegistry.Register(new HeroDef("hero_archer")
            {
                MaxHp = 80f,
                MoveSpeed = 5.5f,
                AttackRange = 8f,
                DefaultWeaponId = "weapon_bow",
                TraitPoolIds = new[] { "pool_bow" },
            });
        }

        /// <summary>注册所有武器原型定义。</summary>
        public static void RegisterWeapons(BattleContext ctx)
        {
            // 单手剑：近距追踪弹，单枪口，攻速较快
            ctx.WeaponDefRegistry.Register(new WeaponDef("weapon_sword")
            {
                BulletType = BulletType.Tracking,
                Damage = 12f,
                BulletSpeed = 14f,
                CooldownSeconds = 0.6f,
                Sockets = new[] { FireSocket.Default },
                TraitPoolIds = new[] { "pool_warrior" },
            });

            // 长弓：直线弹，正前方单枪口，伤害高但攻速慢
            ctx.WeaponDefRegistry.Register(new WeaponDef("weapon_bow")
            {
                BulletType = BulletType.Linear,
                Damage = 18f,
                BulletSpeed = 20f,
                CooldownSeconds = 1.0f,
                Sockets = new[] { FireSocket.Default },
                TraitPoolIds = new[] { "pool_bow" },
            });

            // 双枪：追踪弹，左右双枪口，中等伤害，攻速快
            ctx.WeaponDefRegistry.Register(new WeaponDef("weapon_dual_gun")
            {
                BulletType = BulletType.Tracking,
                Damage = 8f,
                BulletSpeed = 16f,
                CooldownSeconds = 0.5f,
                Sockets = new[] { FireSocket.Left(), FireSocket.Right() },
                TraitPoolIds = new[] { "pool_warrior", "pool_bow" },
            });
        }
    }
}
