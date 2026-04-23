#if UNITY_EDITOR
using System.IO;
using UnityEditor;

namespace Game.Config.Editor.Generator
{
    /// <summary>代码生成侧：确保输出目录存在，并写入阶段性摘要（完整代码生成可在此扩展）。</summary>
    public sealed class ConfigCodeGenerator
    {
        private const string GeneratedPath = "Assets/Game/Config/Generated";

        /// <summary>若不存在则创建 <c>Assets/Game/Config/Generated</c> 目录。</summary>
        public void EnsureGeneratedFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Game/Config"))
            {
                AssetDatabase.CreateFolder("Assets/Game", "Config");
            }

            if (!AssetDatabase.IsValidFolder(GeneratedPath))
            {
                AssetDatabase.CreateFolder("Assets/Game/Config", "Generated");
            }
        }

        /// <summary>写入简单文本摘要，便于确认生成管线已执行（后续可改为 JSON 供 CI 对比）。</summary>
        public void GenerateSummaryStub()
        {
            var filePath = $"{GeneratedPath}/GeneratedSummary.txt";
            File.WriteAllText(filePath, "Phase-1 generation finished for AttrConfigTable.");
        }
    }
}
#endif
