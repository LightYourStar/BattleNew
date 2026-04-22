namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// 武器定义（原型数据）：描述一种武器的全部静态配置，不持有运行时状态。
    /// <para>
    /// 扩展方式：在 Hotfix/配置层创建 <see cref="WeaponDef"/> 实例并调用
    /// <c>WeaponDefRegistry.Register</c> 注册。
    /// </para>
    /// </summary>
    public sealed class WeaponDef
    {
        /// <summary>定义唯一 Id（与 <see cref="Core.BattleLoadout.WeaponDefId"/> 对应）。</summary>
        public string Id { get; }

        // ─── 弹道 ──────────────────────────────────────────────────────────────

        /// <summary>子弹弹道类型。</summary>
        public BulletType BulletType { get; set; } = BulletType.Tracking;

        /// <summary>子弹基础伤害。</summary>
        public float Damage { get; set; } = 10f;

        /// <summary>子弹飞行速度（世界坐标/秒）。</summary>
        public float BulletSpeed { get; set; } = 12f;

        // ─── 发射 ──────────────────────────────────────────────────────────────

        /// <summary>
        /// 攻击冷却时长（秒）：两次发射之间的最短间隔。
        /// 注意：冷却计时存在 <see cref="WeaponRuntime.CooldownRemaining"/>，不在定义里。
        /// </summary>
        public float CooldownSeconds { get; set; } = 0.75f;

        /// <summary>
        /// 枪口挂点列表：每个 Socket 在一次开火中各生成一颗子弹。
        /// 单枪口武器传一个 <see cref="FireSocket.Default"/>；双枪武器传两个。
        /// </summary>
        public FireSocket[] Sockets { get; set; } = { FireSocket.Default };

        // ─── 词条池 ────────────────────────────────────────────────────────────

        /// <summary>
        /// 该武器携带的词条池 Id 列表；战斗开始时与英雄池合并后传给 <c>TraitOfferService</c>。
        /// </summary>
        public string[] TraitPoolIds { get; set; } = System.Array.Empty<string>();

        public WeaponDef(string id)
        {
            Id = id;
        }
    }
}
