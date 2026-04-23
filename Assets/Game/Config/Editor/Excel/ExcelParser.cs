#if UNITY_EDITOR
using System.IO;
using Game.Config.Contracts;
using UnityEngine;

namespace Game.Config.Editor.Excel
{
    /// <summary>
    /// 解析策划表：优先从 <c>Assets/Game/Config/Editor/Excel/Samples/*.csv</c> 读取；缺失时回退到内存 mock。
    /// </summary>
    public sealed class ExcelParser
    {
        private const string SamplesRelative = "Game/Config/Editor/Excel/Samples";

        /// <summary>构建默认校验批次（Attr + 可选 Buff）。</summary>
        public ConfigValidationBatch BuildDefaultBatch()
        {
            var batch = new ConfigValidationBatch();
            var baseDir = Path.Combine(Application.dataPath, SamplesRelative);
            var attrPath = Path.Combine(baseDir, "Attr.csv");
            var buffPath = Path.Combine(baseDir, "Buff.csv");

            if (File.Exists(attrPath))
            {
                batch.Tables.Add(CsvTableReader.Read(attrPath, "Attr.csv", "Attr"));
            }
            else
            {
                batch.Tables.Add(ParseMockAttr());
            }

            if (File.Exists(buffPath))
            {
                batch.Tables.Add(CsvTableReader.Read(buffPath, "Buff.csv", "Buff"));
            }

            return batch;
        }

        /// <summary>内存 mock，与 Attr 表示例列一致。</summary>
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
