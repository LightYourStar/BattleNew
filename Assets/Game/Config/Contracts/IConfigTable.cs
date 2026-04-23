using System.Collections.Generic;

namespace Game.Config.Contracts
{
    /// <summary>
    /// 单张配置表的查询契约：一行对应一个 <typeparamref name="TItem"/>，主键为 <typeparamref name="TKey"/>。
    /// <para>
    /// HybridCLR 边界（稳定层）：查询形状固定；各表具体行类型、字段可在 Generated/热更侧演进。
    /// </para>
    /// </summary>
    public interface IConfigTable<TKey, TItem>
    {
        /// <summary>按键查询；不存在时返回 false。</summary>
        bool TryGet(TKey key, out TItem item);

        /// <summary>按键查询；不存在时由实现抛异常（与 TryGet 二选一使用习惯）。</summary>
        TItem Get(TKey key);

        /// <summary>返回表内全部行（只读视图，避免外部误改内部结构）。</summary>
        IReadOnlyList<TItem> GetAll();
    }
}
