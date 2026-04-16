namespace Game.Battle.Runtime.Services.Config
{
    /// <summary>
    /// 配置读取入口：战斗核心不应直接依赖具体配表系统（Excel/ScriptableObject/Addressables 等）。
    /// </summary>
    public interface IConfigProvider
    {
        /// <summary>
        /// 按键读取配置对象。
        /// <para>
        /// 约定：找不到 key 时由实现决定抛异常或返回默认值；核心层避免吞错导致 silent failure。
        /// </para>
        /// </summary>
        T Get<T>(string key);
    }
}
