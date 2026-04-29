#if UNITY_EDITOR
namespace Game.Config.Editor.Excel
{
    /// <summary>Excel/CSV 读取布局配置（基于有效行索引，0-based）。</summary>
    public sealed class ExcelReadOptions
    {
        public int HeaderRowIndex = 0;
        public int TypeRowIndex = 1;
        public int DataStartRowIndex = 2;
        public bool AutoDetectTypeRowWhenEmpty = true;
    }
}
#endif
