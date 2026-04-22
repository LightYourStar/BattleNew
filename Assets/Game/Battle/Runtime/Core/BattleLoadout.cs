namespace Game.Battle.Runtime.Core
{
    /// <summary>
    /// 战斗配置单：战前选择的英雄、武器、词条池，以及本局随机种子。
    /// <para>
    /// 职责：
    /// <list type="bullet">
    ///   <item><b>播种实体</b>：<c>BattleWorld.SeedFromLoadout</c> 依据此配置创建 HeroEntity 并装配武器。</item>
    ///   <item><b>词条可选集</b>：<c>TraitOfferService</c> 依据 <see cref="TraitPoolIds"/> 生成三选一列表。</item>
    ///   <item><b>回放还原</b>：随 <c>ReplayRecord</c> 一起持久化，保证回放时初始状态与录制完全一致。</item>
    /// </list>
    /// </para>
    /// </summary>
    public sealed class BattleLoadout
    {
        // ─── 英雄 ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 英雄定义 Id（对应 <c>HeroDefRegistry</c> 中的条目）。
        /// 为 null 时 Bootstrap 使用内置默认英雄。
        /// </summary>
        public string? HeroDefId { get; set; }

        // ─── 武器 ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 武器定义 Id（对应 <c>WeaponDefRegistry</c> 中的条目）。
        /// 为 null 时 Bootstrap 使用英雄的默认武器（<c>HeroDef.DefaultWeaponId</c>）。
        /// </summary>
        public string? WeaponDefId { get; set; }

        // ─── 词条池 ────────────────────────────────────────────────────────────

        /// <summary>
        /// 本局允许出现的词条池 Id 列表（对应 <c>TraitPoolRegistry</c> 中的池）。
        /// <para>
        /// 通常由上层（Hotfix / 关卡配置）在战前通过合并英雄池 + 武器池得出：
        /// <code>
        /// loadout.TraitPoolIds = heroDef.TraitPoolIds
        ///     .Concat(weaponDef.TraitPoolIds)
        ///     .Distinct()
        ///     .ToArray();
        /// </code>
        /// </para>
        /// </summary>
        public string[] TraitPoolIds { get; set; } = System.Array.Empty<string>();

        // ─── 随机种子 ─────────────────────────────────────────────────────────

        /// <summary>
        /// 本局 <see cref="BattleRng"/> 的种子。
        /// <para>
        /// 所有战斗内随机（词条 Offer、刷怪位置偏移等）均由此种子的 RNG 驱动，
        /// 确保录制与回放的随机序列完全一致。
        /// 若为 0，Bootstrap 会自动填入当前时间戳种子。
        /// </para>
        /// </summary>
        public long RngSeed { get; set; }

        /// <summary>快速构建：只指定英雄，其余使用默认值。</summary>
        public static BattleLoadout ForHero(string heroDefId, long rngSeed = 0) =>
            new() { HeroDefId = heroDefId, RngSeed = rngSeed };

        /// <summary>快速构建：指定英雄与武器，其余使用默认值。</summary>
        public static BattleLoadout ForHeroAndWeapon(string heroDefId, string weaponDefId, long rngSeed = 0) =>
            new() { HeroDefId = heroDefId, WeaponDefId = weaponDefId, RngSeed = rngSeed };
    }
}
