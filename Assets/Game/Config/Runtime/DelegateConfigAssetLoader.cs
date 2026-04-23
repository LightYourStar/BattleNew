using System;
using Game.Config.Contracts;

namespace Game.Config.Runtime
{
    /// <summary>
    /// 将已有委托包装为 <see cref="IConfigAssetLoader"/>，便于渐进迁移旧代码。
    /// </summary>
    public sealed class DelegateConfigAssetLoader : IConfigAssetLoader
    {
        private readonly Func<string, Type, object> _load;

        public DelegateConfigAssetLoader(Func<string, Type, object> load)
        {
            _load = load ?? throw new ArgumentNullException(nameof(load));
        }

        /// <inheritdoc />
        public object LoadTable(string logicalTableName, Type tableType)
        {
            return _load(logicalTableName, tableType);
        }
    }
}
