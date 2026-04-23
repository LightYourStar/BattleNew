using System;

namespace Game.Config.Contracts
{
    /// <summary>
    /// 当前加载配置的版本元数据：用于热更、AB 包、远端表等多套资源切换与排查。
    /// <para>
    /// 后续接 Addressables/远端：建议用 <see cref="Version"/> 存远端包或 catalog 版本，<see cref="Source"/> 区分
    /// <c>addressables:cfg-main</c> 等；下载完成后本地校验通过再调用 <c>IConfigProvider.Reload</c>，失败则保持旧版本并调用 <c>IConfigProvider.Rollback</c>。
    /// </para>
    /// <para>
    /// HybridCLR 边界（稳定层）：本类型仅承载元数据字段，不含业务解析逻辑。
    /// </para>
    /// </summary>
    [Serializable]
    public sealed class ConfigVersionInfo
    {
        /// <summary>版本号字符串，例如 git 短 hash、构建号、策划表版本号。</summary>
        public string Version;

        /// <summary>来源标识，例如 Resources、Addressables、StreamingAssets。</summary>
        public string Source;

        /// <summary>UTC 时间戳（Ticks），便于日志与回滚对比。</summary>
        public long TimestampUtcTicks;

        /// <summary>工厂方法：创建一条带当前 UTC 时间的版本信息。</summary>
        public static ConfigVersionInfo Create(string version, string source)
        {
            return new ConfigVersionInfo
            {
                Version = version,
                Source = source,
                TimestampUtcTicks = DateTime.UtcNow.Ticks
            };
        }
    }
}
