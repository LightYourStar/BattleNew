#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Game.Config.Contracts;

namespace Game.Config.Editor.Excel
{
    /// <summary>
    /// 读取 OpenXML xlsx（.xlsx）为 <see cref="ConfigValidationInput"/>。
    /// 约定首行是列名。
    /// </summary>
    public static class XlsxTableReader
    {
        private static readonly XNamespace SsNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace RelNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace PkgRelNs = "http://schemas.openxmlformats.org/package/2006/relationships";

        public static ConfigValidationInput Read(string absolutePath, string fileNameForReport, string sheetName)
        {
            var input = new ConfigValidationInput
            {
                FileName = fileNameForReport,
                SheetName = sheetName,
                SourceKind = "xlsx"
            };

            using (var archive = ZipFile.OpenRead(absolutePath))
            {
                var sharedStrings = ReadSharedStrings(archive);
                var sheetEntry = ResolveWorksheetEntry(archive, sheetName, allowFirstSheetFallback: true);
                if (sheetEntry == null)
                {
                    return input;
                }

                using (var stream = sheetEntry.Open())
                {
                    var doc = XDocument.Load(stream);
                    var rows = doc.Root?
                        .Element(SsNs + "sheetData")?
                        .Elements(SsNs + "row")
                        .ToList();
                    if (rows == null || rows.Count == 0)
                    {
                        return input;
                    }

                    var headerMap = ReadHeader(rows[0], sharedStrings);
                    if (headerMap.Count == 0)
                    {
                        return input;
                    }

                    for (var i = 0; i < headerMap.Count; i++)
                    {
                        input.Columns.Add(headerMap[i] ?? string.Empty);
                    }

                    for (var r = 1; r < rows.Count; r++)
                    {
                        var rowValues = ReadRow(rows[r], sharedStrings);
                        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        for (var c = 0; c < headerMap.Count; c++)
                        {
                            var key = headerMap[c];
                            if (string.IsNullOrWhiteSpace(key))
                            {
                                continue;
                            }

                            row[key] = c < rowValues.Count ? rowValues[c] : string.Empty;
                        }

                        input.Rows.Add(row);
                    }
                }
            }

            return input;
        }

        /// <summary>
        /// 仅当目标 sheet 存在时才返回 true；不会回退首张 sheet，适用于多表同文件场景的精准匹配。
        /// </summary>
        public static bool TryReadExactSheet(string absolutePath, string fileNameForReport, string sheetName, out ConfigValidationInput input)
        {
            input = new ConfigValidationInput
            {
                FileName = fileNameForReport,
                SheetName = sheetName,
                SourceKind = "xlsx"
            };

            using (var archive = ZipFile.OpenRead(absolutePath))
            {
                var sharedStrings = ReadSharedStrings(archive);
                var sheetEntry = ResolveWorksheetEntry(archive, sheetName, allowFirstSheetFallback: false);
                if (sheetEntry == null)
                {
                    input = null;
                    return false;
                }

                using (var stream = sheetEntry.Open())
                {
                    var doc = XDocument.Load(stream);
                    var rows = doc.Root?
                        .Element(SsNs + "sheetData")?
                        .Elements(SsNs + "row")
                        .ToList();
                    if (rows == null || rows.Count == 0)
                    {
                        return true;
                    }

                    var headerMap = ReadHeader(rows[0], sharedStrings);
                    for (var i = 0; i < headerMap.Count; i++)
                    {
                        input.Columns.Add(headerMap[i] ?? string.Empty);
                    }

                    for (var r = 1; r < rows.Count; r++)
                    {
                        var rowValues = ReadRow(rows[r], sharedStrings);
                        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        for (var c = 0; c < headerMap.Count; c++)
                        {
                            var key = headerMap[c];
                            if (string.IsNullOrWhiteSpace(key))
                            {
                                continue;
                            }

                            row[key] = c < rowValues.Count ? rowValues[c] : string.Empty;
                        }

                        input.Rows.Add(row);
                    }
                }
            }

            return true;
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            var result = new List<string>();
            var entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null)
            {
                return result;
            }

            using (var stream = entry.Open())
            {
                var doc = XDocument.Load(stream);
                var items = doc.Root?.Elements(SsNs + "si");
                if (items == null)
                {
                    return result;
                }

                foreach (var si in items)
                {
                    var text = string.Concat(si.Descendants(SsNs + "t").Select(t => t.Value));
                    result.Add(text);
                }
            }

            return result;
        }

        private static ZipArchiveEntry ResolveWorksheetEntry(ZipArchive archive, string preferredSheetName, bool allowFirstSheetFallback)
        {
            var wbEntry = archive.GetEntry("xl/workbook.xml");
            var relEntry = archive.GetEntry("xl/_rels/workbook.xml.rels");
            if (wbEntry == null || relEntry == null)
            {
                return null;
            }

            XDocument wbDoc;
            XDocument relDoc;
            using (var wbStream = wbEntry.Open())
            using (var relStream = relEntry.Open())
            {
                wbDoc = XDocument.Load(wbStream);
                relDoc = XDocument.Load(relStream);
            }

            var sheets = wbDoc.Root?.Element(SsNs + "sheets")?.Elements(SsNs + "sheet").ToList();
            if (sheets == null || sheets.Count == 0)
            {
                return null;
            }

            XElement selected = null;
            if (!string.IsNullOrWhiteSpace(preferredSheetName))
            {
                selected = sheets.FirstOrDefault(s =>
                    string.Equals((string)s.Attribute("name"), preferredSheetName, StringComparison.OrdinalIgnoreCase));
            }

            if (selected == null && allowFirstSheetFallback)
            {
                selected = sheets[0];
            }

            if (selected == null)
            {
                return null;
            }
            var rid = (string)selected.Attribute(RelNs + "id");
            if (string.IsNullOrEmpty(rid))
            {
                return null;
            }

            var rel = relDoc.Root?.Elements(PkgRelNs + "Relationship")
                .FirstOrDefault(r => string.Equals((string)r.Attribute("Id"), rid, StringComparison.Ordinal));
            if (rel == null)
            {
                return null;
            }

            var target = (string)rel.Attribute("Target");
            if (string.IsNullOrEmpty(target))
            {
                return null;
            }

            var entryPath = "xl/" + target.TrimStart('/');
            return archive.GetEntry(entryPath.Replace("\\", "/"));
        }

        private static List<string> ReadHeader(XElement rowElement, IReadOnlyList<string> sharedStrings)
        {
            var values = ReadRow(rowElement, sharedStrings);
            for (var i = 0; i < values.Count; i++)
            {
                values[i] = values[i]?.Trim() ?? string.Empty;
            }

            return values;
        }

        private static List<string> ReadRow(XElement rowElement, IReadOnlyList<string> sharedStrings)
        {
            var cells = rowElement.Elements(SsNs + "c");
            var row = new List<string>();
            var expectedColumn = 0;
            foreach (var c in cells)
            {
                var r = (string)c.Attribute("r");
                var col = GetColumnIndexFromCellRef(r);
                while (expectedColumn < col)
                {
                    row.Add(string.Empty);
                    expectedColumn++;
                }

                row.Add(ReadCellText(c, sharedStrings));
                expectedColumn++;
            }

            return row;
        }

        private static int GetColumnIndexFromCellRef(string cellRef)
        {
            if (string.IsNullOrEmpty(cellRef))
            {
                return 0;
            }

            var col = 0;
            for (var i = 0; i < cellRef.Length; i++)
            {
                var ch = cellRef[i];
                if (ch < 'A' || ch > 'Z')
                {
                    break;
                }

                col = col * 26 + (ch - 'A' + 1);
            }

            return Math.Max(col - 1, 0);
        }

        private static string ReadCellText(XElement cell, IReadOnlyList<string> sharedStrings)
        {
            var type = (string)cell.Attribute("t");
            if (type == "inlineStr")
            {
                return cell.Element(SsNs + "is")?.Element(SsNs + "t")?.Value ?? string.Empty;
            }

            var value = cell.Element(SsNs + "v")?.Value ?? string.Empty;
            if (type == "s")
            {
                if (int.TryParse(value, out var index) && index >= 0 && index < sharedStrings.Count)
                {
                    return sharedStrings[index];
                }

                return string.Empty;
            }

            return value;
        }
    }
}
#endif
