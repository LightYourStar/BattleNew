using UnityEngine;

namespace Game.Battle.Runtime.Entities.AI
{
    /// <summary>
    /// AI 状态机状态：当前为最小四态，用于可观测日志与行为分支。
    /// </summary>
    public enum AIState
    {
        /// <summary>未看到目标或没有目标。</summary>
        Idle = 0,

        /// <summary>已看到目标并向其移动。</summary>
        Pursue = 1,

        /// <summary>到达攻击距离并正在攻击。</summary>
        Attack = 2,

        /// <summary>已死亡（用于显式终止追击等逻辑）。</summary>
        Dead = 3,
    }

    /// <summary>
    /// 敌人实体：承载位置、血量、视野、AI 状态与受击反应数据。
    /// <para>
    /// 字段分组说明：
    /// - 移动/AI 状态：由 AISystem 维护。
    /// - 受击反应（击退速度、硬直）：由 HitReactionService 维护。
    /// </para>
    /// </summary>
    public sealed class AIEntity
    {
        // ─── 身份 ──────────────────────────────────────────────────────────────

        /// <summary>实体 Id。</summary>
        public string Id { get; }

        // ─── 位置 ──────────────────────────────────────────────────────────────

        /// <summary>逻辑位置（世界空间）。</summary>
        public Vector3 Position { get; set; }

        // ─── 生命值 ────────────────────────────────────────────────────────────

        /// <summary>最大生命值。</summary>
        public float MaxHp { get; }

        /// <summary>当前生命值。</summary>
        public float CurrentHp { get; private set; }

        /// <summary>是否仍存活。</summary>
        public bool IsAlive => CurrentHp > 0f;

        // ─── 移动 & AI ─────────────────────────────────────────────────────────

        /// <summary>移动速度（单位：世界坐标/秒）。</summary>
        public float MoveSpeed { get; set; } = 2f;

        /// <summary>视野距离：进入该范围会从 Idle 切换到 Pursue。</summary>
        public float SightRange { get; set; } = 8f;

        /// <summary>攻击距离。</summary>
        public float AttackRange { get; set; } = 1.5f;

        /// <summary>攻击冷却（秒）。</summary>
        public float AttackCooldown { get; set; } = 1.0f;

        /// <summary>攻击冷却剩余。</summary>
        public float AttackCooldownRemaining { get; set; }

        /// <summary>单次攻击伤害。</summary>
        public float AttackDamage { get; set; } = 5f;

        /// <summary>当前 AI 状态（由 AISystem.SetState 维护）。</summary>
        public AIState State { get; private set; } = AIState.Idle;

        // ─── 受击反应（由 HitReactionService 维护） ───────────────────────────

        /// <summary>
        /// 击退速度（世界空间，每帧衰减）。
        /// HitReactionService.Tick 每帧推进位置并衰减此值至零。
        /// </summary>
        public Vector3 KnockbackVelocity { get; set; }

        /// <summary>
        /// 受击硬直剩余时长（秒）。
        /// 硬直期间：AISystem 跳过追击与攻击逻辑。
        /// </summary>
        public float StunRemaining { get; set; }

        // ─── 构造 ──────────────────────────────────────────────────────────────

        public AIEntity(string id, Vector3 spawnPosition, float maxHp)
        {
            Id = id;
            Position = spawnPosition;
            MaxHp = maxHp;
            CurrentHp = maxHp;
        }

        // ─── 方法 ──────────────────────────────────────────────────────────────

        /// <summary>受到伤害并夹紧血量下限为 0。</summary>
        public void ReceiveDamage(float damage)
        {
            CurrentHp -= damage;
            if (CurrentHp < 0f)
            {
                CurrentHp = 0f;
            }
        }

        /// <summary>由系统驱动状态切换（避免实体自己随意改状态导致不可追踪）。</summary>
        public void SetState(AIState newState)
        {
            State = newState;
        }
    }
}
