#if UNITY_EDITOR
namespace Game.Config.Editor.Validation
{
    /// <summary>描述一列对外键目标表主键列的引用关系。</summary>
    public sealed class ReferenceRule
    {
        public string FromSheet;
        public string FromColumn;
        public string ToSheet;
        public string ToKeyColumn;

        public ReferenceRule(string fromSheet, string fromColumn, string toSheet, string toKeyColumn = "Id")
        {
            FromSheet = fromSheet;
            FromColumn = fromColumn;
            ToSheet = toSheet;
            ToKeyColumn = toKeyColumn;
        }
    }
}
#endif
