using System.Collections.Generic;
using Game.Battle.Runtime.Entities.AI;
using Game.Battle.Runtime.Entities.Bullet;
using Game.Battle.Runtime.Entities.Hero;

namespace Game.Battle.Runtime.Entities
{
    /// <summary>
    /// 实体注册表：集中管理三类实体列表，并提供按 Id 快速查找的辅助方法。
    /// <para>
    /// 后续演进方向：
    /// - 引入 Dictionary 索引加速查找（当前线性扫描已满足最小闭环需求）。
    /// - 对象池 / 分桶（按阵营、按网格）以支持大规模战斗。
    /// </para>
    /// </summary>
    public sealed class EntityRegistry
    {
        /// <summary>英雄列表（最小闭环默认只放 1 个）。</summary>
        public List<HeroEntity> Heroes { get; } = new();

        /// <summary>敌人列表（波次系统会往这里添加）。</summary>
        public List<AIEntity> Enemies { get; } = new();

        /// <summary>子弹列表（英雄系统发射、子弹系统回收）。</summary>
        public List<BulletEntity> Bullets { get; } = new();

        // ─── 查找辅助 ────────────────────────────────────────────────────────

        /// <summary>按 Id 查找英雄；未找到返回 null。</summary>
        public HeroEntity? FindHero(string id)
        {
            foreach (var h in Heroes)
                if (h.Id == id) return h;
            return null;
        }

        /// <summary>按 Id 查找敌人；未找到返回 null。</summary>
        public AIEntity? FindAI(string id)
        {
            foreach (var e in Enemies)
                if (e.Id == id) return e;
            return null;
        }

        /// <summary>返回第一个存活的英雄；全部阵亡则返回 null。</summary>
        public HeroEntity? FirstAliveHero()
        {
            foreach (var h in Heroes)
                if (h.CurrentHp > 0) return h;
            return null;
        }

        /// <summary>返回第一个存活的敌人；全部阵亡则返回 null。</summary>
        public AIEntity? FirstAliveEnemy()
        {
            foreach (var e in Enemies)
                if (e.IsAlive) return e;
            return null;
        }
    }
}
