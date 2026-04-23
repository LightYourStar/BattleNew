#if UNITY_EDITOR
using System.Collections.Generic;
using Game.Config.Contracts;

namespace Game.Config.Editor.Validation
{
    /// <summary>字段类型校验：对约定为整型的列做 <c>int.TryParse</c>。</summary>
    public sealed class TypeValidator : IConfigValidator
    {
        private static readonly HashSet<string> IntColumns = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
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
            for (var i = 0; i < input.Rows.Count; i++)
            {
                var row = input.Rows[i];
                foreach (var kv in row)
                {
                    if (!IntColumns.Contains(kv.Key))
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
                            i + 2,
                            kv.Key,
                            $"{kv.Key} must be int, got: {kv.Value}");
                    }
                }
            }

            return result;
        }
    }
}
#endif
