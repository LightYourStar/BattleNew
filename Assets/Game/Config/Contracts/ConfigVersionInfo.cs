using System;

namespace Game.Config.Contracts
{
    /// <summary>
    /// 当前加载配置的版本元数据：用于热更、AB 包、远端表等多套资源切换与排查。
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
