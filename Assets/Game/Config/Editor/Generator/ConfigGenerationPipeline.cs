#if UNITY_EDITOR
using System.Collections.Generic;
using Game.Config.Contracts;
using Game.Config.Editor.Excel;
using Game.Config.Editor.Validation;

namespace Game.Config.Editor.Generator
{
    /// <summary>
    /// 编辑器生成总管线：解析 → 多校验器聚合 →（通过则）写摘要与导出 SO。
    /// </summary>
    public sealed class ConfigGenerationPipeline
    {
        private readonly ExcelParser _parser = new ExcelParser();
        private readonly ConfigCodeGenerator _codeGenerator = new ConfigCodeGenerator();
        private readonly ConfigAssetGenerator _assetGenerator = new ConfigAssetGenerator();
        private readonly List<IConfigValidator> _validators = new List<IConfigValidator>
        {
            new PrimaryKeyValidator(),
            new DuplicateKeyValidator(),
            new TypeValidator(),
            new ReferenceValidator()
        };

        /// <summary>执行完整流程；校验失败时提前返回报告，不写资产。</summary>
        public ExcelValidationReport Run()
        {
            var input = _parser.ParseMockAttr();
            var report = new ExcelValidationReport();

            foreach (var validator in _validators)
            {
                var result = validator.Validate(input);
                report.AddRange(result.Errors);
            }

            if (!report.IsValid)
            {
                return report;
            }

            _codeGenerator.EnsureGeneratedFolder();
            _codeGenerator.GenerateSummaryStub();
            _assetGenerator.GenerateAttrAsset();
            return report;
        }
    }
}
#endif
