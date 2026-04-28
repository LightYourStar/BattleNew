#if UNITY_EDITOR
namespace Game.Config.Editor.Types
{
    /// <summary><see cref="FieldTypeParser.ParseFieldType"/> 的输出描述。</summary>
    public readonly struct FieldTypeDescriptor
    {
        public readonly BuiltinFieldKind Kind;
        public readonly string SyntheticEnumName;

        public FieldTypeDescriptor(BuiltinFieldKind kind, string syntheticEnumName = null)
        {
            Kind = kind;
            SyntheticEnumName = syntheticEnumName;
        }

        public bool IsSyntheticEnum => Kind == BuiltinFieldKind.SyntheticEnum;
    }
}
#endif
