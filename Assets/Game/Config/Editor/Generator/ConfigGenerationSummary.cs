#if UNITY_EDITOR
using System;

namespace Game.Config.Editor.Generator
{
    /// <summary>供 UnityEngine.JsonUtility 序列化写入 <c>generation-summary.json</c>（CI 对比）。</summary>
    [Serializable]
    public sealed class ConfigGenerationSummary
    {
        public string generatedAtUtc;
        public string pipelineVersion;
        public bool validationSuccess;
        public int validationErrorCount;
        public TableSummary[] tables;
    }

    [Serializable]
    public sealed class TableSummary
    {
        public string sheetName;
        public string sourceFileName;
        public string sourceKind;
        public int dataRowCount;
    }
}
#endif
