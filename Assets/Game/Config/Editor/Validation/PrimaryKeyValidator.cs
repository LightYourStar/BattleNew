#if UNITY_EDITOR
using Game.Config.Contracts;

namespace Game.Config.Editor.Validation
{
    /// <summary>主键非空校验：当前约定主键列为 Id。</summary>
    public sealed class PrimaryKeyValidator : IConfigValidator
    {
        /// <inheritdoc />
        /// <remarks>行号使用 i+2：假定第 1 行为表头，数据从第 2 行起，与 Excel 行号对齐。</remarks>
        public ConfigValidationResult Validate(ConfigValidationInput input)
        {
            var result = new ConfigValidationResult();
            for (var i = 0; i < input.Rows.Count; i++)
            {
                if (!input.Rows[i].TryGetValue("Id", out var id) || string.IsNullOrWhiteSpace(id))
                {
                    result.AddError(input.FileName, input.SheetName, i + 2, "Id", "Primary key cannot be empty.");
                }
            }

            return result;
        }
    }
}
#endif
