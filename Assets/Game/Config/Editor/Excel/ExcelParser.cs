#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Config.Contracts;
using Game.Config.Editor.Manifest;
using UnityEngine;

namespace Game.Config.Editor.Excel
{
    /// <summary>
    /// 解析策划表：优先在 Samples 下所有 xlsx 中按 sheet 名查表，其次同名 csv，最后回退到内存 mock。
    /// </summary>
    public sealed class ExcelParser
    {
        private const string SamplesRelative = "Game/Config/Editor/Excel/Samples";

        /// <summary>
        /// 按给定表名清单构建批次；表不存在时仅对 Attr 回退 mock，其他表跳过。
        /// </summary>
        public ConfigValidationBatch BuildBatch(IReadOnlyList<string> tableNames, ConfigManifest manifest = null)
        {
            var batch = new ConfigValidationBatch();
            var baseDir = Path.Combine(Application.dataPath, SamplesRelative);
            if (tableNames == null || tableNames.Count == 0)
            {
                return batch;
            }

            foreach (var tableName in tableNames)
            {
                if (string.IsNullOrWhiteSpace(tableName))
                {
                    continue;
                }

                var parsed = TryReadTable(baseDir, tableName.Trim(), BuildReadOptions(manifest, tableName.Trim()));
                if (parsed != null)
                {
                    batch.Tables.Add(parsed);
                    continue;
                }

                if (string.Equals(tableName, "Attr", StringComparison.OrdinalIgnoreCase))
                {
                    batch.Tables.Add(ParseMockAttr());
                }
            }

            return batch;
        }

        /// <summary>扫描 Samples 目录可发现的逻辑表（xlsx sheet + csv 文件名）。</summary>
        public IReadOnlyList<string> DiscoverAvailableTables()
        {
            var baseDir = Path.Combine(Application.dataPath, SamplesRelative);
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!Directory.Exists(baseDir))
            {
                return result.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
            }

            foreach (var xlsxPath in Directory.GetFiles(baseDir, "*.xlsx"))
            {
                foreach (var sheet in XlsxTableReader.GetSheetNames(xlsxPath))
                {
                    if (!string.IsNullOrWhiteSpace(sheet))
                    {
                        result.Add(sheet.Trim());
                    }
                }
            }

            foreach (var csvPath in Directory.GetFiles(baseDir, "*.csv"))
            {
                var table = Path.GetFileNameWithoutExtension(csvPath);
                if (!string.IsNullOrWhiteSpace(table))
                {
                    result.Add(table.Trim());
                }
            }

            return result.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>返回表的可用来源：xlsx/csv/mock/missing（仅用于编辑器展示）。</summary>
        public string ResolveTableSourceKind(string tableName)
        {
            var baseDir = Path.Combine(Application.dataPath, SamplesRelative);
            if (Directory.Exists(baseDir))
            {
                foreach (var xlsxPath in Directory.GetFiles(baseDir, "*.xlsx").OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
                {
                    if (XlsxTableReader.TryReadExactSheet(xlsxPath, Path.GetFileName(xlsxPath), tableName, out _))
                    {
                        return "xlsx";
                    }
                }
            }

            var csvPath = Path.Combine(baseDir, tableName + ".csv");
            if (File.Exists(csvPath))
            {
                return "csv";
            }

            if (string.Equals(tableName, "Attr", StringComparison.OrdinalIgnoreCase))
            {
                return "mock";
            }

            return "missing";
        }

        private static ConfigValidationInput TryReadTable(string baseDir, string tableName, ExcelReadOptions options)
        {
            if (Directory.Exists(baseDir))
            {
                var xlsxFiles = Directory.GetFiles(baseDir, "*.xlsx");
                foreach (var xlsxPath in xlsxFiles.OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
                {
                    if (XlsxTableReader.TryReadExactSheet(xlsxPath, Path.GetFileName(xlsxPath), tableName, out var parsed))
                    {
                        if (options != null)
                        {
                            parsed = XlsxTableReader.Read(xlsxPath, Path.GetFileName(xlsxPath), tableName, options);
                        }
                        return parsed;
                    }
                }
            }

            var csvPath = Path.Combine(baseDir, tableName + ".csv");
            if (File.Exists(csvPath))
            {
                return CsvTableReader.Read(csvPath, tableName + ".csv", tableName, options);
            }

            return null;
        }

        private static ExcelReadOptions BuildReadOptions(ConfigManifest manifest, string tableName)
        {
            if (manifest == null)
            {
                return new ExcelReadOptions();
            }

            var entry = manifest.Tables.FirstOrDefault(t => string.Equals(t.TableName, tableName, StringComparison.OrdinalIgnoreCase));
            if (entry != null && entry.UseCustomLayout)
            {
                return new ExcelReadOptions
                {
                    HeaderRowIndex = entry.HeaderRowIndex,
                    TypeRowIndex = entry.TypeRowIndex,
                    DataStartRowIndex = entry.DataStartRowIndex,
                    AutoDetectTypeRowWhenEmpty = entry.AutoDetectTypeRowWhenEmpty
                };
            }

            return new ExcelReadOptions
            {
                HeaderRowIndex = manifest.GlobalHeaderRowIndex,
                TypeRowIndex = manifest.GlobalTypeRowIndex,
                DataStartRowIndex = manifest.GlobalDataStartRowIndex,
                AutoDetectTypeRowWhenEmpty = manifest.GlobalAutoDetectTypeRowWhenEmpty
            };
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
