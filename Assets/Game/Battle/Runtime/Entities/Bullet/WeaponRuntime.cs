namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// 武器运行时状态：持有一把武器在当前战局中的可变数据（与静态定义 <see cref="WeaponDef"/> 分离）。
    /// <para>
    /// 归属：由 <see cref="Hero.HeroEntity.CurrentWeapon"/> 持有，随英雄生命周期存在。
    /// </para>
    /// <para>
    /// 换武器语义：换武器时换掉 <see cref="Hero.HeroEntity.CurrentWeapon"/> 引用即可；
    /// 旧武器的冷却状态随之丢弃（或可选择继承——此策略由上层决定）。
    /// </para>
    /// </summary>
    public sealed class WeaponRuntime
    {
        /// <summary>对应的武器静态定义。</summary>
        public WeaponDef Def { get; }

        /// <summary>
        /// 当前武器在本局中的实际冷却时长（秒）。
        /// 初始值取自 <see cref="WeaponDef.CooldownSeconds"/>，可被 Buff/Trait 临时修正。
        /// </summary>
        public float CooldownSeconds { get; set; }

        /// <summary>
        /// 攻击冷却剩余时长（秒）。
        /// 由 <see cref="WeaponFireService"/> 每帧递减，触发发射后重置为 <see cref="WeaponDef.CooldownSeconds"/>。
        /// </summary>
        public float CooldownRemaining { get; set; }

        public WeaponRuntime(WeaponDef def)
        {
            Def = def;
            CooldownSeconds = def.CooldownSeconds;
            CooldownRemaining = 0f;
        }
    }
}
