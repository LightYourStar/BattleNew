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
        }
    }
}