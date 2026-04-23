#if UNITY_EDITOR
using System.Collections.Generic;
using Game.Config.Contracts;

namespace Game.Config.Editor.Excel
{
    /// <summary>编辑器窗口展示的校验报告：聚合各 <see cref="IConfigValidator"/> 的错误列表。</summary>
    public sealed class ExcelValidationReport
    {
        public readonly List<ConfigValidationError> Errors = new List<ConfigValidationError>();

        public bool IsValid => Errors.Count == 0;

        /// <summary>批量追加错误（不改变原列表引用）。</summary>
        public void AddRange(IEnumerable<ConfigValidationError> errors)
        {
            Errors.AddRange(errors);
        }
    }
}
#endif
