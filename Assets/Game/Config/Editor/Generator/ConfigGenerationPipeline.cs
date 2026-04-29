#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Game.Config.Contracts;
using Game.Config.Editor.Excel;
using Game.Config.Editor.Manifest;
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

        public sealed class RunResult
        {
            public ExcelValidationReport Report;
            public List<string> ProcessedTables = new List<string>();
            public List<string> MissingTables = new List<string>();
            public long ElapsedMilliseconds;
            public bool ValidateOnly;
        }

        /// <summary>执行完整流程；校验失败时仍写出 JSON 摘要（success=false）便于 CI 归档。</summary>
        public RunResult RunForTables(IReadOnlyList<string> tableNames, ConfigManifest manifest = null, bool validateOnly = false)
        {
            var started = System.DateTime.UtcNow;
            var batch = _parser.BuildBatch(tableNames, manifest);
            var report = new ExcelValidationReport();
            var processed = batch.Tables.Select(t => t.SheetName).ToList();
            var missing = (tableNames ?? new List<string>())
                .Where(t => !processed.Any(p => string.Equals(p, t, System.StringComparison.OrdinalIgnoreCase)))
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToList();

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
                return new RunResult
                {
                    Report = report,
                    ProcessedTables = processed,
                    MissingTables = missing,
                    ElapsedMilliseconds = (long)(System.DateTime.UtcNow - started).TotalMilliseconds,
                    ValidateOnly = validateOnly
                };
            }

            if (!validateOnly)
            {
                _codeGenerator.GenerateTableClasses(batch);
                _assetGenerator.GenerateAssetsForBatch(batch);
            }

            return new RunResult
            {
                Report = report,
                ProcessedTables = processed,
                MissingTables = missing,
                ElapsedMilliseconds = (long)(System.DateTime.UtcNow - started).TotalMilliseconds,
                ValidateOnly = validateOnly
            };
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
