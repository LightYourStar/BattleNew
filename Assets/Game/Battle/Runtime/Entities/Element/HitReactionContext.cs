using UnityEngine;

namespace Game.Battle.Runtime.Entities.Element
{
    /// <summary>
    /// 一次命中的受击反应参数：与伤害数值（<see cref="DamageContext"/>）完全解耦。
    /// <para>
    /// 调用方（HitResolver / AISystem）负责填充参数，
    /// HitReactionService 只负责按参数执行击退/硬直/无敌逻辑，不关心伤害数值。
    /// </para>
    /// </summary>
    public sealed class HitReactionContext
    {
        /// <summary>攻击方 Id（用于日志追踪）。</summary>
        public string AttackerId { get; }

        /// <summary>受击方 Id。</summary>
        public string TargetId { get; }

        /// <summary>
        /// 击退方向（已归一化，世界空间）。
        /// 通常为"攻击方 → 受击方"方向，由调用方在命中确认后计算并传入。
        /// </summary>
        public Vector3 KnockbackDirection { get; }

        /// <summary>
        /// 击退初速度（世界坐标/秒）。
        /// 默认 3f；设为 0 表示该次命中无击退。
        /// </summary>
        public float KnockbackForce { get; set; } = 3f;

        /// <summary>
        /// 受击硬直时长（秒）。
        /// 默认 0.1f；英雄硬直期间不能移动/攻击，AI 硬直期间跳过状态机更新。
        /// </summary>
        public float StunDuration { get; set; } = 0.1f;

        /// <summary>
        /// 无敌帧时长（秒，通常只对英雄生效）。
        /// 默认 0.4f；无敌期间 DamageService 会取消后续伤害。
        /// 设为 0 表示该次命中不触发无敌。
        /// </summary>
        public float InvincibilityDuration { get; set; } = 0.4f;

        public HitReactionContext(string attackerId, string targetId, Vector3 knockbackDirection)
        {
            AttackerId = attackerId;
            TargetId = targetId;
            KnockbackDirection = knockbackDirection.sqrMagnitude > 0f
                ? knockbackDirection.normalized
                : Vector3.back;
        }
    }
}
