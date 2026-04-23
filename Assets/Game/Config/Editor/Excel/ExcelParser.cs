#if UNITY_EDITOR
using Game.Config.Contracts;

namespace Game.Config.Editor.Excel
{
    /// <summary>
    /// 第一阶段解析器：不读真实 xlsx，只构造内存中的 <see cref="ConfigValidationInput"/>，用于跑通「解析 → 校验 → 导出」链路。
    /// 第二阶段可替换为真实 Excel 读取，输出结构保持不变。
    /// </summary>
    public sealed class ExcelParser
    {
        /// <summary>构造 Attr 表示例数据，列与 <see cref="Game.Config.Generated.AttrConfigTable"/> 字段对应。</summary>
        public ConfigValidationInput ParseMockAttr()
        {
            var input = new ConfigValidationInput
            {
                FileName = "MockAttr.xlsx",
                SheetName = "Attr"
            };

            input.Columns.Add("Id");
            input.Columns.Add("Name");
            input.Columns.Add("Value");

            input.Rows.Add(new System.Collections.Generic.Dictionary<string, string>
            {
                ["Id"] = "1",
                ["Name"] = "Attack",
                ["Value"] = "10"
            });

            input.Rows.Add(new System.Collections.Generic.Dictionary<string, string>
            {
                ["Id"] = "2",
                ["Name"] = "Health",
                ["Value"] = "100"
            });

            return input;
        }
    }
}
#endif
