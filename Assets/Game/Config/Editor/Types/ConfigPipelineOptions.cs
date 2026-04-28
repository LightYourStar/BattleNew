#if UNITY_EDITOR
namespace Game.Config.Editor.Types
{
    /// <summary>
    /// 管线级类型选项（编辑器静态配置，后续可改为 ScriptableObject）。
    /// </summary>
    public static class ConfigPipelineOptions
    {
        /// <summary>
        /// 当类型行出现未知 token 时，是否按「枚举」处理（收集取值并生成枚举代码）。
        /// </summary>
        public static bool TreatUnknownTypesAsEnum;

        /// <summary>
        /// 预留：string / string[] 走哈希或索引存储路径（当前仅占位，不改变校验行为）。
        /// </summary>
        public static bool UseHashString;

        /// <summary>
        /// 预留：Color 序列化为 int（当前仅占位，不改变校验行为）。
        /// </summary>
        public static bool CompressColorIntoInt;
    }
}
#endif
