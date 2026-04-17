using UnityEngine;

namespace Game.Battle.Runtime.Entities.Hero
{
    /// <summary>
    /// 英雄实体：承载英雄在逻辑层的全部状态数据。
    /// <para>
    /// 字段分组说明：
    /// - 位置/面向：由 HeroMovementController 维护。
    /// - 状态：由 HeroStateController 维护。
    /// - 锁敌：由 HeroTargetingService 写入。
    /// - 攻击冷却：由 WeaponFireService 维护。
    /// - 受击反应：由 HitReactionService 维护（击退速度、硬直、无敌帧）。
    /// </para>
    /// </summary>
    public sealed class HeroEntity
    {
        // ─── 身份 ──────────────────────────────────────────────────────────────

        /// <summary>实体 Id（用于命令定位与调试追踪）。</summary>
        public string Id { get; }

        // ─── 位置 & 朝向 ───────────────────────────────────────────────────────

        /// <summary>逻辑位置（世界空间）。</summary>
        public Vector3 Position { get; set; }

        /// <summary>当前朝向（归一化，世界空间）。由 HeroMovementController 维护。</summary>
        public Vector3 FacingDirection { get; set; } = Vector3.forward;

        // ─── 生命值 ────────────────────────────────────────────────────────────

        /// <summary>最大生命值。</summary>
        public float MaxHp { get; set; } = 100f;

        /// <summary>当前生命值。</summary>
        public float CurrentHp { get; set; } = 100f;

        /// <summary>是否仍存活（与 AIEntity.IsAlive 语义一致，供 VictoryRule / DeathService 查询）。</summary>
        public bool IsAlive => CurrentHp > 0f;

        // ─── 移动 ──────────────────────────────────────────────────────────────

        /// <summary>移动速度（单位：世界坐标/秒）。</summary>
        public float MoveSpeed { get; set; } = 5f;

        /// <summary>本帧是否收到了移动命令（Hero Tick 尾部清零）。</summary>
        public bool IsMovingThisFrame { get; set; }

        /// <summary>本帧移动方向（原始方向，未归一化）；无移动时为 Vector3.zero。</summary>
        public Vector3 MoveDirectionThisFrame { get; set; }

        // ─── 攻击 ──────────────────────────────────────────────────────────────

        /// <summary>攻击范围：用于最近目标筛选。</summary>
        public float AttackRange { get; set; } = 5f;

        /// <summary>攻击冷却总时长（秒）。</summary>
        public float AttackCooldownSeconds { get; set; } = 0.75f;

        /// <summary>攻击冷却剩余（秒）。</summary>
        public float AttackCooldownRemaining { get; set; }

        // ─── 状态 & 锁敌 ──────────────────────────────────────────────────────

        /// <summary>当前英雄状态（由 HeroStateController 维护）。</summary>
        public HeroState CurrentState { get; set; } = HeroState.Idle;

        /// <summary>当前锁定目标 Id（null 表示无目标，由 HeroTargetingService 写入）。</summary>
        public string? LockedTargetId { get; set; }

        // ─── 受击反应（由 HitReactionService 维护） ───────────────────────────

        /// <summary>
        /// 击退速度（世界空间，每帧衰减）。
        /// HitReactionService.Tick 每帧推进位置并衰减此值至零。
        /// </summary>
        public Vector3 KnockbackVelocity { get; set; }

        /// <summary>
        /// 受击硬直剩余时长（秒）。
        /// 硬直期间：不能移动（BattleWorld.ApplyMove 检查）、不能攻击（WeaponFireService 检查）。
        /// </summary>
        public float StunRemaining { get; set; }

        /// <summary>
        /// 无敌帧剩余时长（秒）。
        /// 无敌期间：DamageService 前置检查会取消伤害（IsCancelled = true）。
        /// </summary>
        public float InvincibleRemaining { get; set; }

        // ─── 构造 ──────────────────────────────────────────────────────────────

        public HeroEntity(string id, Vector3 spawnPosition)
        {
            Id = id;
            Position = spawnPosition;
            AttackCooldownRemaining = 0f;
        }
    }
}
