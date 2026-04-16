using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Entities.Trait
{
    /// <summary>
    /// 词条系统占位：保证 BattleWorld 更新顺序稳定，但不在切片 0/1 引入复杂词条联动。
    /// <para>
    /// 迁移切片对应：切片 8（词条系统）。
    /// </para>
    /// </summary>
    public sealed class TraitSystem
    {
        /// <summary>每逻辑帧更新：当前无操作。</summary>
        public void Tick(BattleContext context, float deltaTime)
        {
            // Reserved for slice 8 implementation.
        }
    }
}
