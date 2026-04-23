using System.Collections.Generic;

namespace Game.Config.Contracts
{
    /// <summary>单次校验的聚合结果：无错误即通过。</summary>
    public sealed class ConfigValidationResult
    {
        /// <summary>本批数据上发现的所有错误（可多条）。</summary>
        public readonly List<ConfigValidationError> Errors = new List<ConfigValidationError>();

        /// <summary>没有任何错误时为 true。</summary>
        public bool IsValid => Errors.Count == 0;

        /// <summary>
        /// 追加一条行级错误；<paramref name="row"/> 建议与 Excel 行号一致（含表头偏移时由调用方换算）。
        /// </summary>
        public void AddError(string file, string sheet, int row, string column, string message)
        {
            Errors.Add(new ConfigValidationError
            {
                FileName = file,
                SheetName = sheet,
                RowNumber = row,
                ColumnName = column,
                Message = message
            });
        }
    }

    /// <summary>单条校验错误：定位到文件 + Sheet + 行 + 列，便于策划按表修改。</summary>
    public sealed class ConfigValidationError
    {
        public string FileName;
        public string SheetName;
        public int RowNumber;
        public string ColumnName;
        public string Message;
    }
}
