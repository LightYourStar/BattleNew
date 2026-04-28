#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using Game.Config.Contracts;
using UnityEngine;

namespace Game.Config.Editor.Excel
{
    /// <summary>
    /// 解析策划表：优先在 Samples 下所有 xlsx 中按 sheet 名查表，其次同名 csv，最后回退到内存 mock。
    /// </summary>
    public sealed class ExcelParser
    {
        private const string SamplesRelative = "Game/Config/Editor/Excel/Samples";

        /// <summary>构建默认校验批次（Attr + 可选 Buff）。</summary>
        public ConfigValidationBatch BuildDefaultBatch()
        {
            var batch = new ConfigValidationBatch();
            var baseDir = Path.Combine(Application.dataPath, SamplesRelative);
            var attr = TryReadTable(baseDir, "Attr");
            batch.Tables.Add(attr ?? ParseMockAttr());

            var buff = TryReadTable(baseDir, "Buff");
            if (buff != null)
            {
                batch.Tables.Add(buff);
            }

            return batch;
        }

        private static ConfigValidationInput TryReadTable(string baseDir, string tableName)
        {
            if (Directory.Exists(baseDir))
            {
                var xlsxFiles = Directory.GetFiles(baseDir, "*.xlsx");
                foreach (var xlsxPath in xlsxFiles.OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
                {
                    if (XlsxTableReader.TryReadExactSheet(xlsxPath, Path.GetFileName(xlsxPath), tableName, out var parsed))
                    {
                        return parsed;
                    }
                }
            }

            var csvPath = Path.Combine(baseDir, tableName + ".csv");
            if (File.Exists(csvPath))
            {
                return CsvTableReader.Read(csvPath, tableName + ".csv", tableName);
            }

            return null;
        }

        /// <summary>内存 mock，与 Attr 表示例列一致。</summary>
        public ConfigValidationInput ParseMockAttr()
        {
            var input = new ConfigValidationInput
            {
                FileName = "MockAttr.xlsx",
                SheetName = "Attr",
                SourceKind = "mock"
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
