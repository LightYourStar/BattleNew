namespace Game.Config.Contracts
{
    /// <summary>
    /// 一次加载或 Reload 的结果：成功时可记录新旧版本；失败时可配合回滚策略恢复上一版本。
    /// </summary>
    public sealed class ConfigLoadResult
    {
        /// <summary>是否加载成功。</summary>
        public bool Success;

        /// <summary>人类可读说明，失败时多为错误原因摘要。</summary>
        public string Message;

        /// <summary>加载完成后当前生效的版本。</summary>
        public ConfigVersionInfo ActiveVersion;

        /// <summary>加载前的版本，便于回滚或对比。</summary>
        public ConfigVersionInfo PreviousVersion;

        /// <summary>构造成功结果。</summary>
        public static ConfigLoadResult Ok(ConfigVersionInfo active, ConfigVersionInfo previous = null)
        {
            return new ConfigLoadResult
            {
                Success = true,
                Message = "Config reload success.",
                ActiveVersion = active,
                PreviousVersion = previous
            };
        }

        /// <summary>构造失败结果；调用方可根据 <paramref name="previous"/> 决定是否回滚。</summary>
        public static ConfigLoadResult Fail(string message, ConfigVersionInfo active, ConfigVersionInfo previous = null)
        {
            return new ConfigLoadResult
            {
                Success = false,
                Message = message,
                ActiveVersion = active,
                PreviousVersion = previous
            };
        }
    }
}
