using System.Collections.Generic;

namespace Game.Config.Runtime
{
    /// <summary>
    /// 已加载表实例的内存缓存，避免同一张表被重复 IO 或反序列化。
    /// <para>HybridCLR 边界（稳定层）：仅做对象字典；缓存的实例类型可来自热更表类。</para>
    /// </summary>
    public sealed class ConfigCache
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

        /// <summary>写入或覆盖某表名的缓存实例。</summary>
        public void Put(string tableName, object table)
        {
            _cache[tableName] = table;
        }

        /// <summary>从缓存取表；类型不匹配时返回 false。</summary>
        public bool TryGet<TTable>(string tableName, out TTable table) where TTable : class
        {
            if (_cache.TryGetValue(tableName, out var obj))
            {
                table = obj as TTable;
                return table != null;
            }

            table = null;
            return false;
        }

        /// <summary>清空全部缓存；通常在热更或 Reload 时调用。</summary>
        public void Clear()
        {
            _cache.Clear();
        }
    }
}
