using UnityEngine;

namespace Game.Battle.Runtime.Entities.Hero
{
    /// <summary>
    /// 英雄实体：当前版本只承载最小闭环需要的数据字段。
    /// <para>
    /// 刻意不做的事情（留给后续切片）：
    /// - 复杂状态机、武器挂载点、技能资源、Buff 容器等。
    /// </para>
    /// </summary>
    public sealed class HeroEntity
    {
        /// <summary>实体 Id（用于命令定位与调试追踪）。</summary>
        public string Id { get; }

        /// <summary>逻辑位置（世界空间）。</summary>
        public Vector3 Position { get; set; }

        /// <summary>移动速度（单位：世界坐标/秒）。</summary>
        public float MoveSpeed { get; set; } = 5f;

        /// <summary>攻击范围：用于最近目标筛选。</summary>
        public float AttackRange { get; set; } = 5f;

        /// <summary>攻击冷却总时长（秒）。</summary>
        public float AttackCooldownSeconds { get; set; } = 0.75f;

        /// <summary>攻击冷却剩余（秒）。</summary>
        public float AttackCooldownRemaining { get; set; }

        public HeroEntity(string id, Vector3 spawnPosition)
        {
            Id = id;
            Position = spawnPosition;
            AttackCooldownRemaining = 0f;
        }
    }
}
