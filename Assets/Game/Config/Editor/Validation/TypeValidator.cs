#if UNITY_EDITOR
using Game.Config.Contracts;

namespace Game.Config.Editor.Validation
{
    /// <summary>字段类型校验：在字符串单元格上尝试 Parse，失败则报行级错误。</summary>
    public sealed class TypeValidator : IConfigValidator
    {
        /// <inheritdoc />
        public ConfigValidationResult Validate(ConfigValidationInput input)
        {
            var result = new ConfigValidationResult();
            for (var i = 0; i < input.Rows.Count; i++)
            {
                var row = input.Rows[i];

                if (row.TryGetValue("Id", out var id) && !int.TryParse(id, out _))
                {
                    result.AddError(input.FileName, input.SheetName, i + 2, "Id", $"Id type invalid: {id}");
                }

                if (row.TryGetValue("Value", out var value) && !int.TryParse(value, out _))
                {
                    result.AddError(input.FileName, input.SheetName, i + 2, "Value", $"Value type invalid: {value}");
                }
            }

            return result;
        }
    }
}
#endif
