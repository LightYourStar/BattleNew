using System;

namespace Game.Config.Contracts
{
    /// <summary>
    /// 配置资产加载抽象：将「逻辑表名 + 期望类型」解析为已加载对象（如 ScriptableObject），便于替换为 Addressables、远端包等。
    /// <para>HybridCLR 边界（稳定层）：接口在 Contracts；具体加载器实现可热更或按平台放在不同程序集。</para>
    /// </summary>
    public interface IConfigAssetLoader
    {
        /// <summary>
        /// 加载一张表对应的资源实例；失败时返回 null（与 <see cref="IConfigProvider.TryGetTable{TTable}"/> 语义一致）。
        /// </summary>
        object LoadTable(string logicalTableName, Type tableType);
    }
}
