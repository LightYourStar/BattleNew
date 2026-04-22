namespace Game.Battle.Runtime.Entities.Hero
{
    /// <summary>
    /// 英雄定义（原型数据）：描述一种英雄的基础属性，不持有任何运行时状态。
    /// <para>
    /// 扩展方式：在 Hotfix/配置层创建 <see cref="HeroDef"/> 实例并调用
    /// <c>HeroDefRegistry.Register</c> 注册，无需改动 Runtime 层代码。
    /// </para>
    /// </summary>
    public sealed class HeroDef
    {
        /// <summary>定义唯一 Id（与 <see cref="Core.BattleLoadout.HeroDefId"/> 对应）。</summary>
        public string Id { get; }

        // ─── 基础属性 ──────────────────────────────────────────────────────────

        /// <summary>最大生命值。</summary>
        public float MaxHp { get; set; } = 100f;

        /// <summary>移动速度（世界坐标/秒）。</summary>
        public float MoveSpeed { get; set; } = 5f;

        /// <summary>攻击范围（用于目标筛选）。</summary>
        public float AttackRange { get; set; } = 5f;

        // ─── 武器 ──────────────────────────────────────────────────────────────

        /// <summary>
        /// 默认武器定义 Id（<see cref="Bullet.WeaponDefRegistry"/> 中的键）。
        /// 若 <see cref="Core.BattleLoadout.WeaponDefId"/> 未指定，则使用此值。
        /// </summary>
        public string DefaultWeaponId { get; set; } = string.Empty;

        // ─── 词条池 ────────────────────────────────────────────────────────────

        /// <summary>
        /// 该英雄携带的词条池 Id 列表；战斗开始时与武器池合并后传给 <c>TraitOfferService</c>。
        /// </summary>
        public string[] TraitPoolIds { get; set; } = System.Array.Empty<string>();

        public HeroDef(string id)
        {
            Id = id;
        }
    }
}
