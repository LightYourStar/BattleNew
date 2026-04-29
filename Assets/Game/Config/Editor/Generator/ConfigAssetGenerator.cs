#if UNITY_EDITOR
using System;
using System.IO;
using Game.Config.Contracts;
using Game.Config.Generated;
using System.Collections.Generic;
using System.Linq;
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
            AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(asset), $"{ResourcesPath}/AttrConfigTable.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 为批次内每张表导出通用 .asset（Data + Resources），保证 Generate 时总有可见资产产物。
        /// </summary>
        public void GenerateAssetsForBatch(ConfigValidationBatch batch)
        {
            if (batch == null)
            {
                return;
            }

            EnsureFolders();
            foreach (var table in batch.Tables)
            {
                if (table == null || string.IsNullOrWhiteSpace(table.SheetName))
                {
                    continue;
                }

                var fileName = $"{table.SheetName}ConfigRaw.asset";
                var dataPath = $"{DataPath}/{fileName}";
                var resPath = $"{ResourcesPath}/{fileName}";

                var source = BuildGenericAssetData(table);
                UpsertAsset(dataPath, source);
                UpsertAsset(resPath, source);
                UnityEngine.Object.DestroyImmediate(source);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GenericConfigTableAsset BuildGenericAssetData(ConfigValidationInput table)
        {
            var asset = ScriptableObject.CreateInstance<GenericConfigTableAsset>();
            var rows = table.Rows
                .Select(r =>
                {
                    var rr = new GenericConfigRow();
                    foreach (var c in table.Columns)
                    {
                        rr.Cells.Add(r.TryGetValue(c, out var v) ? v : string.Empty);
                    }

                    return rr;
                })
                .ToList();

            var types = table.ColumnTypeTokens.Count == table.Columns.Count
                ? new List<string>(table.ColumnTypeTokens)
                : new List<string>(Enumerable.Repeat(string.Empty, table.Columns.Count));

            asset.ResetFrom(
                table.SheetName,
                new List<string>(table.Columns),
                types,
                rows);
            return asset;
        }

        private static void UpsertAsset(string path, GenericConfigTableAsset source)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GenericConfigTableAsset>(path);
            if (existing == null)
            {
                var any = AssetDatabase.LoadMainAssetAtPath(path);
                if (any != null)
                {
                    AssetDatabase.DeleteAsset(path);
                }

                var created = UnityEngine.Object.Instantiate(source);
                created.name = Path.GetFileNameWithoutExtension(path);
                AssetDatabase.CreateAsset(created, path);
                return;
            }

            EditorUtility.CopySerialized(source, existing);
            EditorUtility.SetDirty(existing);
        }
    }
}
#endif
