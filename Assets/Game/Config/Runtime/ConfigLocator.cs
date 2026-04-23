using Game.Config.Contracts;

namespace Game.Config.Runtime
{
    /// <summary>
    /// 按「表名 + 主键」定位单行：在已有 <see cref="IConfigProvider"/> 之上的一层薄封装，业务可读性更好。
    /// </summary>
    public sealed class ConfigLocator
    {
        private readonly IConfigProvider _provider;

        /// <param name="provider">运行时配置入口，通常由启动代码构造并注入。</param>
        public ConfigLocator(IConfigProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// 先取整张表再按键查询；表不存在时由 <see cref="IConfigProvider.GetTable{TTable}"/> 抛异常。
        /// 若希望表缺失也不抛异常，可先对 Provider 使用 TryGetTable，再自行 TryGet。
        /// </summary>
        public bool TryGet<TKey, TItem, TTable>(string tableName, TKey key, out TItem item)
            where TTable : class, IConfigTable<TKey, TItem>
        {
            var table = _provider.GetTable<TTable>(tableName);
            return table.TryGet(key, out item);
        }
    }
}
