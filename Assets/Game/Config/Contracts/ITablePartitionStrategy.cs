namespace Game.Config.Contracts
{
    /// <summary>
    /// 大表分片扩展点（预留）：按主键或业务维度决定行落在哪个物理分片/子资产，用于加载与查询路由。
    /// <para>
    /// HybridCLR：分片策略与路由规则可热更；接口留在 Contracts 供稳定层依赖抽象。
    /// </para>
    /// </summary>
    public interface ITablePartitionStrategy
    {
        /// <summary>返回逻辑表名对应的物理资源名或分片 id（具体语义由加载器约定）。</summary>
        string GetPartitionId(string tableName, string primaryKeyString);
    }
}
