using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Entities.Buff
{
    /// <summary>
    /// Buff 系统占位：保证 BattleWorld 更新顺序稳定，但不在切片 0/1 引入复杂 Buff 联动。
    /// <para>
    /// 迁移切片对应：切片 7（Buff 系统）。
    /// </para>
    /// </summary>
    public sealed class BuffSystem
    {
        /// <summary>每逻辑帧更新：当前无操作。</summary>
        public void Tick(BattleContext context, float deltaTime)
        {
            // Reserved for slice 7 implementation.
        }
    }
}
