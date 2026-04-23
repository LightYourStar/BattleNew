using System;
using System.Collections.Generic;

namespace Game.Config.Contracts
{
    /// <summary>
    /// 一次生成/校验会话中的多张表数据；外键校验器需要跨表访问。
    /// </summary>
    public sealed class ConfigValidationBatch
    {
        public readonly List<ConfigValidationInput> Tables = new List<ConfigValidationInput>();

        /// <summary>按 <see cref="ConfigValidationInput.SheetName"/> 查找（忽略大小写）。</summary>
        public ConfigValidationInput FindBySheetName(string sheetName)
        {
            foreach (var t in Tables)
            {
                if (string.Equals(t.SheetName, sheetName, StringComparison.OrdinalIgnoreCase))
                {
                    return t;
                }
            }

            return null;
        }
    }
}
