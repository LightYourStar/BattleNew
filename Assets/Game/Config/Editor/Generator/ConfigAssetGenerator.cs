#if UNITY_EDITOR
using Game.Config.Generated;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Config.Editor.Generator
{
    /// <summary>
    /// 配置资产导出：生成 ScriptableObject；一份放 Config/Data 归档，一份放 Resources 供运行时示例加载。
    /// </summary>
    public sealed class ConfigAssetGenerator
    {
        private const string DataPath = "Assets/Game/Config/Data";
        private const string ResourcesPath = "Assets/Resources/Config";

        /// <summary>确保 Data 与 Resources/Config 目录存在。</summary>
        public void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Game/Config"))
            {
                AssetDatabase.CreateFolder("Assets/Game", "Config");
            }

            if (!AssetDatabase.IsValidFolder(DataPath))
            {
                AssetDatabase.CreateFolder("Assets/Game/Config", "Data");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(ResourcesPath))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Config");
            }
        }

        /// <summary>创建 Attr 示例表资产并保存到工程内固定路径。</summary>
        public void GenerateAttrAsset()
        {
            EnsureFolders();
            var asset = ScriptableObject.CreateInstance<AttrConfigTable>();
            asset.ResetItems(new List<AttrConfigItem>
            {
                new AttrConfigItem { Id = 1, Name = "Attack", Value = 10 },
                new AttrConfigItem { Id = 2, Name = "Health", Value = 100 }
            });
            AssetDatabase.CreateAsset(asset, $"{DataPath}/AttrConfigTable.asset");
            AssetDatabase.CreateAsset(Object.Instantiate(asset), $"{ResourcesPath}/AttrConfigTable.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif
