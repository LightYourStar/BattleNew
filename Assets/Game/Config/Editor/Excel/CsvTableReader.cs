#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Game.Config.Contracts;

namespace Game.Config.Editor.Excel
{
    /// <summary>
    /// 从 UTF-8 CSV 读取为 <see cref="ConfigValidationInput"/>：首行为列名，不含 BOM 时亦可；单元格内勿含未转义逗号。
    /// </summary>
    public static class CsvTableReader
    {
        /// <summary>
        /// <paramref name="sheetName"/> 写入 <see cref="ConfigValidationInput.SheetName"/>，用于外键规则与错误定位。
        /// </summary>
        public static ConfigValidationInput Read(string absolutePath, string fileNameForReport, string sheetName)
        {
            return Read(absolutePath, fileNameForReport, sheetName, new ExcelReadOptions());
        }

        public static ConfigValidationInput Read(string absolutePath, string fileNameForReport, string sheetName, ExcelReadOptions options)
        {
            var raw = File.ReadAllText(absolutePath, Encoding.UTF8);
            if (raw.Length > 0 && raw[0] == '\uFEFF')
            {
                raw = raw.TrimStart('\uFEFF');
            }

            var lines = raw.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var effectiveRows = new List<List<string>>();
            for (var li = 0; li < lines.Length; li++)
            {
                var line = lines[li].Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }
                effectiveRows.Add(SplitCsvLine(line));
            }

            return BuildInputFromRows(effectiveRows, fileNameForReport, sheetName, "csv", options);
        }

        private static ConfigValidationInput BuildInputFromRows(
            List<List<string>> rows,
            string fileNameForReport,
            string sheetName,
            string sourceKind,
            ExcelReadOptions options)
        {
            options ??= new ExcelReadOptions();
            var input = new ConfigValidationInput
            {
                FileName = fileNameForReport,
                SheetName = sheetName,
                SourceKind = sourceKind,
                AllowAutoDetectTypeRow = options.AutoDetectTypeRowWhenEmpty
            };

            if (rows.Count == 0 || options.HeaderRowIndex < 0 || options.HeaderRowIndex >= rows.Count)
            {
                return input;
            }

            var headers = rows[options.HeaderRowIndex];
            input.Columns.AddRange(headers);

            if (options.TypeRowIndex >= 0 && options.TypeRowIndex < rows.Count)
            {
                var typeRow = rows[options.TypeRowIndex];
                for (var i = 0; i < input.Columns.Count; i++)
                {
                    input.ColumnTypeTokens.Add(i < typeRow.Count ? typeRow[i].Trim() : string.Empty);
                }
            }

            var dataStart = options.DataStartRowIndex;
            if (dataStart < 0)
            {
                dataStart = options.HeaderRowIndex + 1;
            }

            for (var ri = dataStart; ri < rows.Count; ri++)
            {
                var cells = rows[ri];
                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (var c = 0; c < headers.Count; c++)
                {
                    row[headers[c]] = c < cells.Count ? cells[c] : string.Empty;
                }

                input.Rows.Add(row);
            }

            return input;
        }

        private static List<string> SplitCsvLine(string line)
        {
            // Minimal CSV parser:
            // - supports quoted fields: "a,b"
            // - supports escaped quote inside quoted field: ""
            var parts = new List<string>();
            var sb = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    continue;
                }

                if (ch == ',' && !inQuotes)
                {
                    parts.Add(sb.ToString().Trim());
                    sb.Clear();
                    continue;
                }

                sb.Append(ch);
            }

            parts.Add(sb.ToString().Trim());
            return parts;
        }
    }
}
#endif
