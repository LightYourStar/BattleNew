#if UNITY_EDITOR
namespace Game.Config.Editor.Types
{
    /// <summary>类型行解析后的内置类别（含数组与合成枚举占位）。</summary>
    public enum BuiltinFieldKind
    {
        None = 0,
        Int,
        Float,
        Double,
        Long,
        String,
        Vector2,
        Vector3,
        Vector4,
        Rect,
        Color,
        IntArray,
        LongArray,
        FloatArray,
        DoubleArray,
        StringArray,
        SyntheticEnum
    }
}
#endif
