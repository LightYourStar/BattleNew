using System;
using Game.Config.Contracts;

namespace Game.Config.Runtime
{
    /// <summary>
    /// 运行时配置提供者：组合「注册表 + 内存缓存 + 可注入加载器」，与具体资源形态（SO、AB、二进制）解耦。
    /// <para>
    /// HybridCLR 边界（稳定层）：本类与 <see cref="ConfigRegistry"/>、<see cref="ConfigCache"/> 保持精简；
    /// 表内容类型、Addressables 加载细节、远端 manifest 解析等放在可热更或注入的 <see cref="IConfigAssetLoader"/> 实现一侧。
    /// </para>
    /// </summary>
    public sealed class ConfigProvider : IConfigProvider
    {
        private readonly ConfigRegistry _registry;
        private readonly ConfigCache _cache;
        private readonly IConfigAssetLoader _assetLoader;
        private ConfigVersionInfo _activeVersion;

        /// <summary>最近一次 <see cref="Reload"/> 之前生效的版本；供 <see cref="Rollback"/> 一步恢复，单次回滚后清空。</summary>
        private ConfigVersionInfo _rollbackAnchor;

        /// <summary>
        /// <paramref name="registry"/>：表名到类型的映射；
        /// <paramref name="assetLoader"/>：按表名与实际类型加载资源对象；
        /// <paramref name="initialVersion"/>：初始版本信息，可为 null 时使用默认 bootstrap 版本。
        /// </summary>
        public ConfigProvider(ConfigRegistry registry, IConfigAssetLoader assetLoader, ConfigVersionInfo initialVersion = null)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _cache = new ConfigCache();
            _assetLoader = assetLoader ?? throw new ArgumentNullException(nameof(assetLoader));
            _activeVersion = initialVersion ?? ConfigVersionInfo.Create("bootstrap", "local");
        }

        /// <summary>使用委托作为加载器的便捷构造（内部包装为 <see cref="DelegateConfigAssetLoader"/>）。</summary>
        public ConfigProvider(ConfigRegistry registry, Func<string, Type, object> tableLoader, ConfigVersionInfo initialVersion = null)
            : this(registry, new DelegateConfigAssetLoader(tableLoader), initialVersion)
        {
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

            var loaded = _assetLoader.LoadTable(tableName, tableType);
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
        /// 清空缓存并更新版本；将旧版本写入回滚锚点。若新资源拉取失败，宿主应调用 <see cref="Rollback"/> 或不要替换版本。
        /// 后续可在此接入：先下载/校验再切换版本，失败则不修改 <see cref="_activeVersion"/>。
        /// </remarks>
        public ConfigLoadResult Reload(ConfigVersionInfo versionInfo)
        {
            if (versionInfo == null)
            {
                throw new ArgumentNullException(nameof(versionInfo));
            }

            var previous = _activeVersion;
            _cache.Clear();
            _rollbackAnchor = previous;
            _activeVersion = versionInfo;
            return ConfigLoadResult.Ok(_activeVersion, previous);
        }

        /// <inheritdoc />
        public ConfigLoadResult Rollback()
        {
            if (_rollbackAnchor == null)
            {
                return ConfigLoadResult.Fail("尚未执行过 Reload，无可用回滚锚点。", _activeVersion, null);
            }

            var abandoned = _activeVersion;
            var restore = _rollbackAnchor;
            _rollbackAnchor = null;
            _cache.Clear();
            _activeVersion = restore;
            return ConfigLoadResult.Ok(_activeVersion, abandoned);
        }
    }
}
