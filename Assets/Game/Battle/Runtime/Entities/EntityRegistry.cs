using System.Collections.Generic;
using Game.Battle.Runtime.Entities.AI;
using Game.Battle.Runtime.Entities.Bullet;
using Game.Battle.Runtime.Entities.Hero;

namespace Game.Battle.Runtime.Entities
{
    /// <summary>
    /// 实体注册表：当前阶段用 List 做最小闭环演示。
    /// <para>
    /// 后续演进方向：
    /// - 引入实体 Id 索引（Dictionary）以支持快速查询。
    /// - 引入对象池/分桶（按阵营、按网格）以优化大规模战斗。
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
    }
}
