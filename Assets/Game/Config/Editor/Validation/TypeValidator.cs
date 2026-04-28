#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Game.Config.Contracts;
using Game.Config.Editor.Types;

namespace Game.Config.Editor.Validation
{
    /// <summary>
    /// 字段类型校验：若存在类型行则按矩阵校验；否则回退到旧版 int 列白名单逻辑。
    /// </summary>
    public sealed class TypeValidator : IConfigValidator
    {
        private static readonly HashSet<string> LegacyIntColumns = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "Id",
            "Value",
            "AttrId",
            "BuffId",
            "SkillId",
        };

        /// <inheritdoc />
        public ConfigValidationResult Validate(ConfigValidationInput input)
        {
            var result = new ConfigValidationResult();
            if (input.ColumnTypeTokens.Count == input.Columns.Count && input.ColumnTypeTokens.Count > 0)
            {
                ValidateWithTypeRow(input, result);
            }
            else
            {
                ValidateLegacyIntWhitelist(input, result);
            }

            return result;
        }

        private static void ValidateWithTypeRow(ConfigValidationInput input, ConfigValidationResult result)
        {
            var descriptors = new FieldTypeDescriptor[input.Columns.Count];
            for (var c = 0; c < input.Columns.Count; c++)
            {
                if (!FieldTypeParser.TryParseFieldType(
                        input.ColumnTypeTokens[c],
                        ConfigPipelineOptions.TreatUnknownTypesAsEnum,
                        out descriptors[c],
                        out var err))
                {
                    result.AddError(input.FileName, input.SheetName, 2, input.Columns[c], $"Invalid type token: {err}");
                }
            }

            var syntheticAllowed = new Dictionary<int, HashSet<string>>();
            for (var c = 0; c < input.Columns.Count; c++)
            {
                if (descriptors[c].IsSyntheticEnum)
                {
                    syntheticAllowed[c] = CollectDistinctColumnValues(input, input.Columns[c]);
                }
            }

            foreach (var kv in syntheticAllowed)
            {
                var colIndex = kv.Key;
                var colName = input.Columns[colIndex];
                var groups = new Dictionary<string, List<string>>(System.StringComparer.OrdinalIgnoreCase);
                foreach (var raw in kv.Value)
                {
                    var key = SyntheticEnumNaming.BaseMemberKey(raw);
                    if (!groups.TryGetValue(key, out var list))
                    {
                        list = new List<string>();
                        groups[key] = list;
                    }

                    if (!list.Any(x => string.Equals(x, raw, System.StringComparison.Ordinal)))
                    {
                        list.Add(raw);
                    }
                }

                foreach (var g in groups.Values)
                {
                    if (g.Count <= 1)
                    {
                        continue;
                    }

                    result.AddError(
                        input.FileName,
                        input.SheetName,
                        2,
                        colName,
                        $"Synthetic enum member collision after sanitize: {string.Join(", ", g)}");
                }
            }

            for (var r = 0; r < input.Rows.Count; r++)
            {
                var row = input.Rows[r];
                for (var c = 0; c < input.Columns.Count; c++)
                {
                    var col = input.Columns[c];
                    if (!row.TryGetValue(col, out var cell))
                    {
                        cell = string.Empty;
                    }

                    var desc = descriptors[c];
                    if (desc.Kind == BuiltinFieldKind.None)
                    {
                        continue;
                    }

                    if (desc.IsSyntheticEnum)
                    {
                        var t = cell?.Trim() ?? string.Empty;
                        if (string.IsNullOrEmpty(t))
                        {
                            result.AddError(input.FileName, input.SheetName, input.FirstDataExcelRow + r, col, "Synthetic enum cell cannot be empty.");
                            continue;
                        }

                        if (!syntheticAllowed[c].Contains(t))
                        {
                            result.AddError(input.FileName, input.SheetName, input.FirstDataExcelRow + r, col, $"Value '{t}' is not in declared enum domain for column.");
                        }

                        continue;
                    }

                    if (!CellValueValidator.TryValidate(cell, desc, out var vErr))
                    {
                        result.AddError(input.FileName, input.SheetName, input.FirstDataExcelRow + r, col, vErr);
                    }
                }
            }
        }

        private static HashSet<string> CollectDistinctColumnValues(ConfigValidationInput input, string column)
        {
            var set = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (var row in input.Rows)
            {
                if (row.TryGetValue(column, out var v) && !string.IsNullOrWhiteSpace(v))
                {
                    set.Add(v.Trim());
                }
            }

            return set;
        }

        private static void ValidateLegacyIntWhitelist(ConfigValidationInput input, ConfigValidationResult result)
        {
            for (var i = 0; i < input.Rows.Count; i++)
            {
                var row = input.Rows[i];
                foreach (var kv in row)
                {
                    if (!LegacyIntColumns.Contains(kv.Key))
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(kv.Value))
                    {
                        continue;
                    }

                    if (!int.TryParse(kv.Value, out _))
                    {
                        result.AddError(
                            input.FileName,
                            input.SheetName,
                            input.FirstDataExcelRow + i,
                            kv.Key,
                            $"{kv.Key} must be int, got: {kv.Value}");
                    }
                }
            }
        }
    }
}
#endif
