namespace Game.Config.Contracts
{
    /// <summary>
    /// 运行时配置统一入口：业务只依赖本接口，便于日后换成 Addressables、二进制包等实现。
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
        /// 按版本信息重新加载配置；当前为预留入口，可扩展为清缓存、换资源包、失败回滚等。
        /// </summary>
        ConfigLoadResult Reload(ConfigVersionInfo versionInfo);
    }
}
