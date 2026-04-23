#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Config.Editor.Generator
{
    /// <summary>代码生成侧：确保输出目录存在，并写入 CI 用 JSON 摘要与人读 txt。</summary>
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

        /// <summary>写入 <c>generation-summary.json</c>，供 CI 或脚本对比生成是否变化。</summary>
        public void WriteCiSummaryJson(ConfigGenerationSummary summary)
        {
            EnsureGeneratedFolder();
            var json = JsonUtility.ToJson(summary, prettyPrint: true);
            var filePath = $"{GeneratedPath}/generation-summary.json";
            File.WriteAllText(filePath, json);
        }

        /// <summary>写入一行文本摘要，便于人工快速确认管线已跑。</summary>
        public void GenerateSummaryStub(bool validationSuccess)
        {
            var filePath = $"{GeneratedPath}/GeneratedSummary.txt";
            var line = validationSuccess
                ? "Config generation OK (see generation-summary.json)."
                : "Config generation validation failed (see generation-summary.json).";
            File.WriteAllText(filePath, line);
        }
    }
}
#endif
