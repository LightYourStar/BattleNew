#if UNITY_EDITOR
namespace Game.Config.Editor.Validation
{
    /// <summary>项目级外键规则清单；随表增加在此追加规则。</summary>
    public static class ConfigReferenceRules
    {
        public static readonly ReferenceRule[] Default =
        {
            new ReferenceRule("Buff", "AttrId", "Attr", "Id"),
        };
    }
}
#endif
