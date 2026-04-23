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

        /// <summary>列名列表（可选，用于后续扩展列顺序校验）。</summary>
        public List<string> Columns = new List<string>();

        /// <summary>数据行；每行字典的 key 为列名，value 为原始字符串（类型校验时再 Parse）。</summary>
        public List<Dictionary<string, string>> Rows = new List<Dictionary<string, string>>();
    }
}
