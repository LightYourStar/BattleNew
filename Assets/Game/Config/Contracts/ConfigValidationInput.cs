using System.Collections.Generic;

namespace Game.Config.Contracts
{
    /// <summary>
    /// 校验阶段的通用输入：从 Excel 或其它来源解析后，统一成“列名 -> 单元格字符串”的行列表。
    /// </summary>
    public sealed class ConfigValidationInput
    {
        /// <summary>来源文件名，用于错误定位（展示给策划/程序）。</summary>
        public string FileName;

        /// <summary>Sheet 名，用于错误定位。</summary>
        public string SheetName;

        /// <summary>数据来源类型（例如 xlsx/csv/mock），用于生成摘要与调试。</summary>
        public string SourceKind = "unknown";

        /// <summary>列名列表（可选，用于后续扩展列顺序校验）。</summary>
        public List<string> Columns = new List<string>();

        /// <summary>
        /// 可选类型行：与 <see cref="Columns"/> 一一对应；由 <c>TypeRowProcessor</c> 从首行数据剥离后填充。
        /// </summary>
        public List<string> ColumnTypeTokens = new List<string>();

        /// <summary>当 <see cref="ColumnTypeTokens"/> 为空时，是否允许自动从首条数据行识别类型行。</summary>
        public bool AllowAutoDetectTypeRow = true;

        /// <summary>
        /// Excel 中第一条数据行的行号（1-based）：无类型行为 2；有类型行为 3（表头 + 类型行）。
        /// </summary>
        public int FirstDataExcelRow => ColumnTypeTokens.Count > 0 ? 3 : 2;

        /// <summary>数据行；每行字典的 key 为列名，value 为原始字符串（类型校验时再 Parse）。</summary>
        public List<Dictionary<string, string>> Rows = new List<Dictionary<string, string>>();
    }
}
