#if UNITY_EDITOR
using System.Collections.Generic;
using Game.Config.Contracts;

namespace Game.Config.Editor.Validation
{
    /// <summary>
    /// 跨表外键校验：根据 <see cref="ReferenceRule"/> 检查引用列值是否存在于目标表主键集合。
    /// </summary>
    public sealed class ReferenceValidator
    {
        /// <summary>单表接口兼容（本校验器为批处理，此处返回空结果）。</summary>
        public ConfigValidationResult Validate(ConfigValidationInput input)
        {
            return new ConfigValidationResult();
        }

        /// <summary>
        /// 对批次内所有规则校验；若某规则涉及的表不在批次中则跳过该规则。
        /// </summary>
        public ConfigValidationResult ValidateBatch(ConfigValidationBatch batch, ReferenceRule[] rules)
        {
            var result = new ConfigValidationResult();
            if (batch == null || rules == null)
            {
                return result;
            }

            foreach (var rule in rules)
            {
                var from = batch.FindBySheetName(rule.FromSheet);
                var to = batch.FindBySheetName(rule.ToSheet);
                if (from == null || to == null)
                {
                    continue;
                }

                var keySet = new HashSet<string>(System.StringComparer.Ordinal);
                foreach (var row in to.Rows)
                {
                    if (row.TryGetValue(rule.ToKeyColumn, out var key) && !string.IsNullOrWhiteSpace(key))
                    {
                        keySet.Add(key.Trim());
                    }
                }

                for (var i = 0; i < from.Rows.Count; i++)
                {
                    var row = from.Rows[i];
                    if (!row.TryGetValue(rule.FromColumn, out var fk) || string.IsNullOrWhiteSpace(fk))
                    {
                        continue;
                    }

                    fk = fk.Trim();
                    if (!keySet.Contains(fk))
                    {
                        result.AddError(
                            from.FileName,
                            from.SheetName,
                            from.FirstDataExcelRow + i,
                            rule.FromColumn,
                            $"Foreign key not found in {rule.ToSheet}.{rule.ToKeyColumn}: {fk}");
                    }
                }
            }

            return result;
        }
    }
}
#endif
