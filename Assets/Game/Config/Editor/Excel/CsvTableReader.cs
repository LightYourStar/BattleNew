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
            var raw = File.ReadAllText(absolutePath, Encoding.UTF8);
            if (raw.Length > 0 && raw[0] == '\uFEFF')
            {
                raw = raw.TrimStart('\uFEFF');
            }

            var lines = raw.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var input = new ConfigValidationInput
            {
                FileName = fileNameForReport,
                SheetName = sheetName
            };

            var headerLineIndex = -1;
            for (var li = 0; li < lines.Length; li++)
            {
                var line = lines[li].Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                headerLineIndex = li;
                break;
            }

            if (headerLineIndex < 0)
            {
                return input;
            }

            var headers = SplitCsvLine(lines[headerLineIndex]);
            input.Columns.AddRange(headers);

            for (var ri = headerLineIndex + 1; ri < lines.Length; ri++)
            {
                var line = lines[ri].Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                var cells = SplitCsvLine(line);
                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (var c = 0; c < headers.Count && c < cells.Count; c++)
                {
                    row[headers[c]] = cells[c];
                }

                input.Rows.Add(row);
            }

            return input;
        }

        private static List<string> SplitCsvLine(string line)
        {
            var parts = line.Split(',');
            for (var i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
            }

            return new List<string>(parts);
        }
    }
}
#endif
