namespace Game.Config.Contracts
{
    /// <summary>
    /// 运行时配置统一入口：业务只依赖本接口，便于日后换成 Addressables、二进制包等实现。
    /// <para>
    /// HybridCLR 边界（稳定层）：与 <see cref="IConfigTable{TKey,TItem}"/>、版本类型共同构成「高频读取、少热更」契约；
    /// 具体加载实现、表内容解析、多语言等放在可热更层，通过注入接入。
    /// </para>
    /// </summary>
    public interface IConfigProvider
    {
        /// <summary>
        /// 按表名取整张表；找不到或类型不匹配时由实现抛异常（适合“必须存在”的读取）。
        /// </summary>
        TTable GetTable<TTable>(string tableName) where TTable : class;

        /// <summary>
        /// 按表名尝试取表；失败返回 false，不抛异常（适合可选配置或降级逻辑）。
        /// </summary>
        bool TryGetTable<TTable>(string tableName, out TTable table) where TTable : class;

        /// <summary>
        /// 按版本信息重新加载配置：会清空内存表缓存，下次访问时按新资源重新加载。
        /// 若拉表失败，宿主应在未替换版本前调用 <see cref="Rollback"/>，或不要提交新版本。
        /// </summary>
        ConfigLoadResult Reload(ConfigVersionInfo versionInfo);

        /// <summary>
        /// 回滚到最近一次 <see cref="Reload"/> 之前的版本元数据，并清空缓存以便按旧版本重新加载。
        /// 仅一步：再次 Reload 会刷新可回滚锚点；从未 Reload 过则失败。
        /// </summary>
        ConfigLoadResult Rollback();
    }
}
