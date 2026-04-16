using UnityEngine;

namespace Game.Battle.Runtime.Entities.AI
{
    /// <summary>
    /// AI 状态机状态：当前为最小三态，用于可观测日志与行为分支。
    /// </summary>
    public enum AIState
    {
        /// <summary>未看到目标或没有目标。</summary>
        Idle = 0,

        /// <summary>已看到目标并向其移动。</summary>
        Pursue = 1,

        /// <summary>已死亡（用于显式终止追击等逻辑）。</summary>
        Dead = 2,
    }

    /// <summary>
    /// 敌人实体：承载位置、血量、视野与 AI 状态。
    /// </summary>
    public sealed class AIEntity
    {
        /// <summary>实体 Id。</summary>
        public string Id { get; }

        /// <summary>逻辑位置（世界空间）。</summary>
        public Vector3 Position { get; set; }

        /// <summary>最大生命值。</summary>
        public float MaxHp { get; }

        /// <summary>当前生命值。</summary>
        public float CurrentHp { get; private set; }

        /// <summary>移动速度（单位：世界坐标/秒）。</summary>
        public float MoveSpeed { get; set; } = 2f;

        /// <summary>视野距离：进入该范围会从 Idle 切换到 Pursue。</summary>
        public float SightRange { get; set; } = 8f;

        /// <summary>当前 AI 状态。</summary>
        public AIState State { get; private set; } = AIState.Idle;

        /// <summary>是否仍存活。</summary>
        public bool IsAlive => CurrentHp > 0f;

        public AIEntity(string id, Vector3 spawnPosition, float maxHp)
        {
            Id = id;
            Position = spawnPosition;
            MaxHp = maxHp;
            CurrentHp = maxHp;
        }

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
