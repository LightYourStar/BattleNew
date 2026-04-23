#if UNITY_EDITOR
using System.Collections.Generic;
using Game.Config.Contracts;

namespace Game.Config.Editor.Validation
{
    /// <summary>主键唯一性校验：同一 Id 出现第二次即报错。</summary>
    public sealed class DuplicateKeyValidator : IConfigValidator
    {
        /// <inheritdoc />
        public ConfigValidationResult Validate(ConfigValidationInput input)
        {
            var result = new ConfigValidationResult();
            var seen = new HashSet<string>();

            for (var i = 0; i < input.Rows.Count; i++)
            {
                if (!input.Rows[i].TryGetValue("Id", out var id) || string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                if (!seen.Add(id))
                {
                    result.AddError(input.FileName, input.SheetName, i + 2, "Id", $"Duplicate key: {id}");
                }
            }

            return result;
        }
    }
}
#endif
