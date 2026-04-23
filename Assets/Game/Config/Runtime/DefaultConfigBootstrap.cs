using Game.Config.Contracts;
using Game.Config.Generated;
using UnityEngine;

namespace Game.Config.Runtime
{
    /// <summary>
    /// 默认启动装配：注册示例表，并用 Unity <see cref="Resources"/> 从 <c>Resources/Config/</c> 下按表名加载。
    /// </summary>
    public static class DefaultConfigBootstrap
    {
        /// <summary>与 Resources 路径和资源文件名一致的逻辑表名。</summary>
        public const string AttrTableName = "AttrConfigTable";

        /// <summary>
        /// 创建基于 Resources 的 <see cref="ConfigProvider"/>；适合学习示例与单机最小闭环。
        /// </summary>
        public static ConfigProvider CreateForResources()
        {
            var registry = new ConfigRegistry();
            registry.Register<AttrConfigTable>(AttrTableName);

            return new ConfigProvider(
                registry,
                (tableName, _) => Resources.Load<ScriptableObject>($"Config/{tableName}"),
                ConfigVersionInfo.Create("v0", "Resources"));
        }
    }
}
