namespace Game.Config.Contracts
{
    /// <summary>
    /// 多语言字段解析扩展点（预留）：将表内语言键或占位符解析为展示用文本。
    /// <para>
    /// HybridCLR：实现可放在热更程序集；本接口定义在 Contracts 稳定层，避免运行时核心频繁变更。
    /// </para>
    /// </summary>
    public interface ITextLocalizationResolver
    {
        /// <summary>将原始表字段（如 textId 或内嵌 key）解析为当前语言下的字符串。</summary>
        string Resolve(string rawValue, string columnName, string tableName);
    }
}
