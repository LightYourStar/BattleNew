using Game.Battle.Runtime.Entities.Bullet;
using UnityEngine;

namespace Game.Battle.Runtime.Entities.Hero
{
    /// <summary>
    /// 英雄实体：承载英雄在逻辑层的全部运行时状态。
    /// <para>
    /// 字段分组：
    /// - 身份/定义：Id、DefId（关联 HeroDef 原型）。
    /// - 位置/面向：由 HeroMovementController 维护。
    /// - 生命值：由 DamageService / DeathService 维护。
    /// - 移动：由 HeroMovementController 写入，HeroSystem 尾帧清零。
    /// - 武器：CurrentWeapon（WeaponRuntime）持有武器实例状态，由 Bootstrap/SeedFromLoadout 装配。
    /// - 攻击范围：由 HeroDef 初始化，可被 Trait OnEquip 修改。
    /// - 锁敌：由 HeroTargetingService 写入。
    /// - 受击反应：由 HitReactionService 维护（击退速度、硬直、无敌帧）。
    /// </para>
    /// </summary>
    public sealed class HeroEntity
    {
        // ─── 身份 ──────────────────────────────────────────────────────────────

        /// <summary>运行时唯一 Id（命令定位、调试追踪用）。</summary>
        public string Id { get; }

        /// <summary>
        /// 英雄定义 Id（关联 <see cref="HeroDef"/>）。
        /// 由 <c>SeedFromLoadout</c> 写入；为空字符串表示使用内置默认值。
        /// </summary>
        public string DefId { get; set; } = string.Empty;

        // ─── 位置 & 朝向 ───────────────────────────────────────────────────────

        /// <summary>逻辑位置（世界空间）。</summary>
        public Vector3 Position { get; set; }

        /// <summary>当前朝向（归一化）。由 HeroMovementController 维护。</summary>
        public Vector3 FacingDirection { get; set; } = Vector3.forward;

        // ─── 生命值 ────────────────────────────────────────────────────────────

        /// <summary>最大生命值。</summary>
        public float MaxHp { get; set; } = 100f;

        /// <summary>当前生命值。</summary>
        public float CurrentHp { get; set; } = 100f;

        /// <summary>是否仍存活（与 AIEntity.IsAlive 语义一致）。</summary>
        public bool IsAlive => CurrentHp > 0f;

        // ─── 移动 ──────────────────────────────────────────────────────────────

        /// <summary>移动速度（世界坐标/秒）。</summary>
        public float MoveSpeed { get; set; } = 5f;

        /// <summary>本帧是否收到了移动命令（Hero Tick 尾部清零）。</summary>
        public bool IsMovingThisFrame { get; set; }

        /// <summary>本帧移动方向（原始，未归一化）；无移动时为 Vector3.zero。</summary>
        public Vector3 MoveDirectionThisFrame { get; set; }

        // ─── 武器 ──────────────────────────────────────────────────────────────

        /// <summary>
        /// 当前装备的武器运行时实例。
        /// 由 <c>SeedFromLoadout</c> 根据 Loadout.WeaponDefId（或 HeroDef.DefaultWeaponId）创建。
        /// null 表示手无寸铁（不会触发发射）。
        /// </summary>
        public WeaponRuntime? CurrentWeapon { get; set; }

        // ─── 攻击范围 ─────────────────────────────────────────────────────────

        /// <summary>
        /// 攻击范围：用于最近目标筛选。
        /// 由 HeroDef.AttackRange 初始化，可被 Trait（OnEquip/OnUnequip）动态修改。
        /// </summary>
        public float AttackRange { get; set; } = 5f;

        // ─── 状态 & 锁敌 ──────────────────────────────────────────────────────

        /// <summary>当前英雄状态（由 HeroStateController 维护）。</summary>
        public HeroState CurrentState { get; set; } = HeroState.Idle;

        /// <summary>当前锁定目标 Id（null 表示无目标）。由 HeroTargetingService 写入。</summary>
        public string? LockedTargetId { get; set; }

        // ─── 受击反应（由 HitReactionService 维护） ───────────────────────────

        /// <summary>击退速度（世界空间，每帧衰减）。</summary>
        public Vector3 KnockbackVelocity { get; set; }

        /// <summary>受击硬直剩余时长（秒）。硬直期间不能移动、不能攻击。</summary>
        public float StunRemaining { get; set; }

        /// <summary>无敌帧剩余时长（秒）。无敌期间 DamageService 自动取消伤害。</summary>
        public float InvincibleRemaining { get; set; }

        // ─── 构造 ──────────────────────────────────────────────────────────────

        public HeroEntity(string id, Vector3 spawnPosition)
        {
            Id = id;
            Position = spawnPosition;
        }
    }
}
