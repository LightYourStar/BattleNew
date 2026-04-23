using Game.Config.Contracts;
using Game.Config.Generated;
using UnityEngine;

namespace Game.Config.Runtime
{
    /// <summary>
    /// 运行时最小示例：演示从 <see cref="IConfigProvider"/> 取表，以及 <see cref="IConfigTable{TKey,TItem}"/> 的 TryGet / Get 差异。
    /// </summary>
    public sealed class ConfigRuntimeExample : MonoBehaviour
    {
        private void Start()
        {
            var provider = DefaultConfigBootstrap.CreateForResources();
            var table = provider.GetTable<AttrConfigTable>(DefaultConfigBootstrap.AttrTableName);

            if (table.TryGet(1, out var item))
            {
                Debug.Log($"[ConfigRuntimeExample] TryGet success: id={item.Id}, value={item.Value}");
            }
            else
            {
                Debug.LogWarning("[ConfigRuntimeExample] TryGet failed for id=1.");
            }

            try
            {
                AttrConfigItem byGet = table.Get(1);
                Debug.Log($"[ConfigRuntimeExample] Get success: {byGet.Name}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ConfigRuntimeExample] Get failed: {ex.Message}");
            }

            // Reload / Rollback：演示版本锚点与缓存清空（实际项目里应在拉表成功后再 Reload）
            var reloadResult = provider.Reload(ConfigVersionInfo.Create("v1", "Resources"));
            Debug.Log($"[ConfigRuntimeExample] Reload -> {reloadResult.ActiveVersion.Version}");

            var rollbackResult = provider.Rollback();
            if (rollbackResult.Success)
            {
                Debug.Log($"[ConfigRuntimeExample] Rollback -> {rollbackResult.ActiveVersion.Version}");
            }
            else
            {
                Debug.LogWarning($"[ConfigRuntimeExample] Rollback: {rollbackResult.Message}");
            }
        }
    }
}