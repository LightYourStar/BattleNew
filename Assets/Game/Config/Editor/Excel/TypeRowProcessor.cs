#if UNITY_EDITOR
using System.Collections.Generic;
using Game.Config.Contracts;
using Game.Config.Editor.Types;

namespace Game.Config.Editor.Excel
{
    /// <summary>
    /// 若首行数据可被完整解析为类型行，则提升到 <see cref="ConfigValidationInput.ColumnTypeTokens"/> 并从数据中移除该行。
    /// </summary>
    public static class TypeRowProcessor
    {
        public static void TryConsumeTypeRow(ConfigValidationInput input)
        {
            if (input == null || input.Columns.Count == 0 || input.Rows.Count == 0)
            {
                return;
            }

            if (input.ColumnTypeTokens.Count > 0)
            {
                return;
            }

            var first = input.Rows[0];
            var tokens = new List<string>(input.Columns.Count);
            foreach (var col in input.Columns)
            {
                if (!first.TryGetValue(col, out var raw))
                {
                    return;
                }

                tokens.Add(raw?.Trim() ?? string.Empty);
            }

            var treatEnum = ConfigPipelineOptions.TreatUnknownTypesAsEnum;
            for (var i = 0; i < tokens.Count; i++)
            {
                if (!FieldTypeParser.TryParseFieldType(tokens[i], treatEnum, out _, out _))
                {
                    return;
                }
            }

            input.ColumnTypeTokens.AddRange(tokens);
            input.Rows.RemoveAt(0);
        }
    }
}
#endif
