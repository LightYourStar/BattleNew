using System;
using Game.Config.Contracts;
using UnityEngine;

namespace Game.Config.Runtime
{
    /// <summary>
    /// 使用 Unity <see cref="Resources"/> 加载配置资产：<c>Resources/{folder}/{logicalTableName}</c>。
    /// </summary>
    public sealed class ResourcesConfigAssetLoader : IConfigAssetLoader
    {
        private readonly string _folder;

        /// <param name="folder">Resources 下子目录，不含前后斜杠，例如 <c>Config</c>。</param>
        public ResourcesConfigAssetLoader(string folder = "Config")
        {
            _folder = string.IsNullOrEmpty(folder) ? "Config" : folder.Trim('/');
        }

        /// <inheritdoc />
        public object LoadTable(string logicalTableName, Type tableType)
        {
            if (string.IsNullOrEmpty(logicalTableName))
            {
                return null;
            }

            var path = $"{_folder}/{logicalTableName}";
            return Resources.Load(path, tableType);
        }
    }
}
