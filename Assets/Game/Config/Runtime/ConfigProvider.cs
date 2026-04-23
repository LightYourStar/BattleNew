using System;
using Game.Config.Contracts;

namespace Game.Config.Runtime
{
    /// <summary>
    /// 运行时配置提供者：组合「注册表 + 内存缓存 + 可注入加载器」，与具体资源形态（SO、AB、二进制）解耦。
    /// </summary>
    public sealed class ConfigProvider : IConfigProvider
    {
        private readonly ConfigRegistry _registry;
        private readonly ConfigCache _cache;
        private readonly Func<string, Type, object> _tableLoader;
        private ConfigVersionInfo _activeVersion;

        /// <summary>
        /// <paramref name="registry"/>：表名到类型的映射；
        /// <paramref name="tableLoader"/>：按表名与实际类型加载资源对象（例如 Resources.Load）；
        /// <paramref name="initialVersion"/>：初始版本信息，可为 null 时使用默认 bootstrap 版本。
        /// </summary>
        public ConfigProvider(ConfigRegistry registry, Func<string, Type, object> tableLoader, ConfigVersionInfo initialVersion = null)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _cache = new ConfigCache();
            _tableLoader = tableLoader ?? throw new ArgumentNullException(nameof(tableLoader));
            _activeVersion = initialVersion ?? ConfigVersionInfo.Create("bootstrap", "local");
        }

        /// <inheritdoc />
        public TTable GetTable<TTable>(string tableName) where TTable : class
        {
            if (!TryGetTable<TTable>(tableName, out var table))
            {
                throw new InvalidOperationException($"Config table not found or type mismatch: {tableName}");
            }

            return table;
        }

        /// <inheritdoc />
        public bool TryGetTable<TTable>(string tableName, out TTable table) where TTable : class
        {
            if (_cache.TryGet(tableName, out table))
            {
                return true;
            }

            if (!_registry.TryGetTableType(tableName, out var tableType))
            {
                table = null;
                return false;
            }

            var loaded = _tableLoader(tableName, tableType);
            if (loaded is TTable typed)
            {
                _cache.Put(tableName, typed);
                table = typed;
                return true;
            }

            table = null;
            return false;
        }

        /// <inheritdoc />
        /// <remarks>
        /// 当前实现：清空缓存并更新版本号；后续可在此触发重新从磁盘/网络拉表，失败时用 <see cref="ConfigLoadResult.PreviousVersion"/> 回滚。
        /// </remarks>
        public ConfigLoadResult Reload(ConfigVersionInfo versionInfo)
        {
            var previous = _activeVersion;
            _cache.Clear();
            _activeVersion = versionInfo;
            return ConfigLoadResult.Ok(_activeVersion, previous);
        }
    }
}
