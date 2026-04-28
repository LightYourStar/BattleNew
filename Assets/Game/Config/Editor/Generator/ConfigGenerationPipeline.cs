#if UNITY_EDITOR
using System.Collections.Generic;
using Game.Config.Contracts;
using Game.Config.Editor.Excel;
using Game.Config.Editor.Validation;

namespace Game.Config.Editor.Generator
{
    /// <summary>
    /// 编辑器生成总管线：解析批次 → 逐表基础校验 → 外键批校验 →（通过则）写摘要与导出 SO。
    /// </summary>
    public sealed class ConfigGenerationPipeline
    {
        private readonly ExcelParser _parser = new ExcelParser();
        private readonly ConfigCodeGenerator _codeGenerator = new ConfigCodeGenerator();
        private readonly ConfigAssetGenerator _assetGenerator = new ConfigAssetGenerator();
        private readonly ReferenceValidator _referenceValidator = new ReferenceValidator();

        private readonly List<IConfigValidator> _singleTableValidators = new List<IConfigValidator>
        {
            new PrimaryKeyValidator(),
            new DuplicateKeyValidator(),
            new TypeValidator(),
        };

        /// <summary>执行完整流程；校验失败时仍写出 JSON 摘要（success=false）便于 CI 归档。</summary>
        public ExcelValidationReport Run()
        {
            var batch = _parser.BuildDefaultBatch();
            var report = new ExcelValidationReport();

            foreach (var table in batch.Tables)
            {
                TypeRowProcessor.TryConsumeTypeRow(table);
            }

            SyntheticEnumGenerator.GenerateForBatch(batch);

            foreach (var table in batch.Tables)
            {
                foreach (var validator in _singleTableValidators)
                {
                    report.AddRange(validator.Validate(table).Errors);
                }
            }

            report.AddRange(_referenceValidator.ValidateBatch(batch, ConfigReferenceRules.Default).Errors);

            var summary = BuildSummary(batch, report);
            _codeGenerator.EnsureGeneratedFolder();
            _codeGenerator.WriteCiSummaryJson(summary);
            _codeGenerator.GenerateSummaryStub(report.IsValid);

            if (!report.IsValid)
            {
                return report;
            }

            _assetGenerator.GenerateAttrAsset();
            return report;
        }

        private static ConfigGenerationSummary BuildSummary(ConfigValidationBatch batch, ExcelValidationReport report)
        {
            var tables = new List<TableSummary>();
            foreach (var t in batch.Tables)
            {
                tables.Add(new TableSummary
                {
                    sheetName = t.SheetName,
                    sourceFileName = t.FileName,
                    sourceKind = string.IsNullOrEmpty(t.SourceKind) ? "unknown" : t.SourceKind,
                    dataRowCount = t.Rows.Count
                });
            }

            return new ConfigGenerationSummary
            {
                generatedAtUtc = System.DateTime.UtcNow.ToString("o"),
                pipelineVersion = "3",
                validationSuccess = report.IsValid,
                validationErrorCount = report.Errors.Count,
                tables = tables.ToArray()
            };
        }
    }
}
#endif
