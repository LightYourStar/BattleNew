#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Game.Config.Editor.Excel;
using UnityEditor;

namespace Game.Config.Editor.Manifest
{
    /// <summary>Manifest 读写与扫描同步。</summary>
    public static class ConfigManifestService
    {
        public const string ManifestAssetPath = "Assets/Game/Config/Editor/ConfigManifest.asset";

        public static ConfigManifest LoadOrCreate()
        {
            var manifest = AssetDatabase.LoadAssetAtPath<ConfigManifest>(ManifestAssetPath);
            if (manifest != null)
            {
                return manifest;
            }

            EnsureParentFolders();
            manifest = UnityEngine.ScriptableObject.CreateInstance<ConfigManifest>();
            AssetDatabase.CreateAsset(manifest, ManifestAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return manifest;
        }

        public static IReadOnlyList<string> GetEnabledTableNames(ConfigManifest manifest)
        {
            return manifest.Tables
                .Where(t => t.Enabled && !string.IsNullOrWhiteSpace(t.TableName))
                .Select(t => t.TableName.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static void SyncWithDiscovered(ConfigManifest manifest, IReadOnlyList<string> discovered)
        {
            foreach (var t in manifest.Tables)
            {
                if (t.TableName != null)
                {
                    t.TableName = t.TableName.Trim();
                }
            }

            var set = new HashSet<string>(
                (discovered ?? Array.Empty<string>()).Select(x => (x ?? string.Empty).Trim()).Where(x => x.Length > 0),
                StringComparer.OrdinalIgnoreCase);

            foreach (var table in set.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                var exists = manifest.Tables.Any(t => string.Equals(t.TableName, table, StringComparison.OrdinalIgnoreCase));
                if (!exists)
                {
                    manifest.Tables.Add(new ConfigTableEntry
                    {
                        TableName = table,
                        Enabled = true
                    });
                }
            }

            var merged = new Dictionary<string, ConfigTableEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in manifest.Tables.Where(x => !string.IsNullOrWhiteSpace(x.TableName)))
            {
                var key = t.TableName.Trim();
                if (!merged.TryGetValue(key, out var keep))
                {
                    t.TableName = key;
                    merged[key] = t;
                    continue;
                }

                keep.Enabled |= t.Enabled;
                keep.UseCustomLayout |= t.UseCustomLayout;
            }

            manifest.Tables = merged.Values
                .OrderBy(t => t.TableName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            EditorUtility.SetDirty(manifest);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureParentFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Game/Config/Editor"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Game/Config"))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Game"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Game");
                    }

                    AssetDatabase.CreateFolder("Assets/Game", "Config");
                }

                AssetDatabase.CreateFolder("Assets/Game/Config", "Editor");
            }
        }
    }
}
#endif
