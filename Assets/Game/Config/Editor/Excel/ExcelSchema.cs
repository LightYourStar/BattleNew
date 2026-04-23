#if UNITY_EDITOR
using System.Collections.Generic;

namespace Game.Config.Editor.Excel
{
    /// <summary>描述一张 Sheet 的结构化视图（列 + 行），供后续真实解析器与校验器衔接使用。</summary>
    public sealed class ExcelSchema
    {
        public string FileName;
        public string SheetName;
        public List<string> Columns = new List<string>();
        public List<ExcelRow> Rows = new List<ExcelRow>();
    }

    /// <summary>一行数据：带 Excel 行号，单元格为列名到字符串。</summary>
    public sealed class ExcelRow
    {
        public int RowNumber;
        public Dictionary<string, string> Cells = new Dictionary<string, string>();
    }
}
#endif
