using System;
using System.Collections.Generic;

namespace Game.Config.Runtime
{
    /// <summary>
    /// 表注册表：把「逻辑表名」映射到「C# 表类型」，供 <see cref="ConfigProvider"/> 在加载时知道要转成哪种类型。
    /// </summary>
    public sealed class ConfigRegistry
    {
        private readonly Dictionary<string, Type> _tableTypes = new Dictionary<string, Type>(StringComparer.Ordinal);

        /// <summary>注册一张表：<paramref name="tableName"/> 需与加载器里使用的资源名一致。</summary>
        public void Register<TTable>(string tableName) where TTable : class
        {
            _tableTypes[tableName] = typeof(TTable);
        }

        /// <summary>查询已注册表的 CLR 类型；未注册返回 false。</summary>
        public bool TryGetTableType(string tableName, out Type type)
        {
            return _tableTypes.TryGetValue(tableName, out type);
        }
    }
}
