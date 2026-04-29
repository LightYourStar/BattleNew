#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Config.Editor.Excel;
using Game.Config.Editor.Generator;
using Game.Config.Editor.Manifest;
using UnityEditor;
using UnityEngine;

namespace Game.Config.Editor.Window
{
    /// <summary>Unity 菜单打开的生成窗口：触发管线并展示校验错误列表。</summary>
    public sealed class ConfigGenerationWindow : EditorWindow
    {
        private string _status = "Idle";
        private Vector2 _tableScroll;
        private Vector2 _errorScroll;
        private string _errors = string.Empty;
        private ConfigManifest _manifest;
        private readonly HashSet<string> _highlighted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _lastTableStatus = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _sourceKind = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string _search = string.Empty;
        private bool _showOnlyFailed;
        private string _summary = "Last run: — | Processed: 0 | Success: 0 | Failed: 0 | 0ms";
        private bool _showAdvanced;
        private bool _showSelectionTools;

        /// <summary>菜单入口：<c>Tools/Config/Generation Window</c>。</summary>
        [MenuItem("Tools/Config/Generation Window")]
        private static void Open()
        {
            var window = GetWindow<ConfigGenerationWindow>();
            window.titleContent = new GUIContent("Config Generator");
            window.Show();
        }

        private void OnEnable()
        {
            LoadManifestAndSync();
        }

        /// <summary>绘制窗口 UI：按钮 + 状态 + 错误滚动区。</summary>
        private void OnGUI()
        {
            GUILayout.Label("Config Generation Pipeline", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh Table List From Samples"))
            {
                LoadManifestAndSync();
            }

            EditorGUILayout.Space();
            DrawSummaryBar();

            EditorGUILayout.Space();
            DrawAdvancedSettings();

            EditorGUILayout.Space();
            DrawToolbar();

            EditorGUILayout.Space();
            DrawSelectionToolsFoldout();

            EditorGUILayout.Space();
            DrawTableList();

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Generate", GUILayout.Height(28f)))
                {
                    RunPipeline(ConfigManifestService.GetEnabledTableNames(_manifest), validateOnly: false);
                }

                if (GUILayout.Button("Validate", GUILayout.Height(28f)))
                {
                    RunPipeline(ConfigManifestService.GetEnabledTableNames(_manifest), validateOnly: true);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Status", _status);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Copy Errors", GUILayout.Width(100f)))
                {
                    EditorGUIUtility.systemCopyBuffer = _errors ?? string.Empty;
                }

                GUILayout.FlexibleSpace();
            }

            _errorScroll = EditorGUILayout.BeginScrollView(_errorScroll, GUILayout.Height(220f));
            EditorGUILayout.TextArea(_errors);
            EditorGUILayout.EndScrollView();
        }

        /// <summary>按指定表清单执行生成，并展示结果。</summary>
        private void RunPipeline(IReadOnlyList<string> tableNames, bool validateOnly)
        {
            if (tableNames == null || tableNames.Count == 0)
            {
                _status = "No Tables Selected";
                _errors = "Please select at least one table or enable tables in manifest.";
                Repaint();
                return;
            }

            ConfigGenerationPipeline.RunResult runResult;
            try
            {
                var pipeline = new ConfigGenerationPipeline();
                runResult = pipeline.RunForTables(tableNames, _manifest, validateOnly);
            }
            catch (IOException ex) when (IsLikelyFileLock(ex))
            {
                _status = "File locked";
                _errors =
                    "The CSV or workbook may still be open in Excel (exclusive lock). Close it or save a copy, then retry.\n\n" +
                    ex;
                Repaint();
                return;
            }
            catch (Exception ex)
            {
                _status = "Pipeline Exception";
                _errors = ex.ToString();
                Repaint();
                return;
            }
            var report = runResult.Report;
            var processed = string.Join(", ", runResult.ProcessedTables);
            var missing = string.Join(", ", runResult.MissingTables);

            foreach (var t in tableNames)
            {
                _lastTableStatus[t] = "Skipped";
            }

            foreach (var t in runResult.ProcessedTables)
            {
                _lastTableStatus[t] = report.IsValid ? (validateOnly ? "Validated" : "Generated") : "Failed";
            }

            foreach (var t in runResult.MissingTables)
            {
                _lastTableStatus[t] = "Missing";
            }

            _summary =
                $"Last run: {(validateOnly ? "Validate" : "Generate")} | Processed: {runResult.ProcessedTables.Count} | Success: {(report.IsValid ? runResult.ProcessedTables.Count : 0)} | Failed: {(report.IsValid ? 0 : report.Errors.Count)} | {runResult.ElapsedMilliseconds}ms";
            if (report.IsValid)
            {
                _status = "Success";
                _errors = $"Mode: {(validateOnly ? "ValidateOnly" : "Generate")}\nProcessed tables: {processed}\nNo validation errors.";
                if (!string.IsNullOrEmpty(missing))
                {
                    _errors += $"\nMissing tables: {missing}";
                }
            }
            else
            {
                _status = "Validation Failed";
                var details = string.Join(
                    "\n",
                    report.Errors.ConvertAll(e => $"{e.FileName}/{e.SheetName} r{e.RowNumber} c{e.ColumnName}: {e.Message}"));
                _errors = $"Mode: {(validateOnly ? "ValidateOnly" : "Generate")}\nProcessed tables: {processed}\n{details}";
                if (!string.IsNullOrEmpty(missing))
                {
                    _errors += $"\nMissing tables: {missing}";
                }
            }

            Repaint();
        }

        private void DrawSummaryBar()
        {
            EditorGUILayout.HelpBox(_summary, MessageType.None);
        }

        private static bool IsLikelyFileLock(IOException ex)
        {
            var m = ex.Message ?? string.Empty;
            return m.IndexOf("Sharing violation", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   m.IndexOf("being used by another process", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   m.IndexOf("cannot access the file", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _search = EditorGUILayout.TextField("Search", _search);
                _showOnlyFailed = EditorGUILayout.ToggleLeft("Show Failed Only", _showOnlyFailed, GUILayout.Width(140f));
            }
        }

        private void DrawSelectionToolsFoldout()
        {
            if (_manifest == null)
            {
                return;
            }

            _showSelectionTools = EditorGUILayout.Foldout(_showSelectionTools, "Selection & highlighted batch", true);
            if (!_showSelectionTools)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Highlight All", GUILayout.Width(100f)))
                    {
                        foreach (var t in _manifest.Tables)
                        {
                            _highlighted.Add(t.TableName);
                        }
                    }

                    if (GUILayout.Button("Clear Highlight", GUILayout.Width(100f)))
                    {
                        _highlighted.Clear();
                    }

                    if (GUILayout.Button("Invert Highlight", GUILayout.Width(110f)))
                    {
                        var all = _manifest.Tables.Select(t => t.TableName).ToList();
                        var next = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var t in all)
                        {
                            if (!_highlighted.Contains(t))
                            {
                                next.Add(t);
                            }
                        }

                        _highlighted.Clear();
                        foreach (var t in next)
                        {
                            _highlighted.Add(t);
                        }
                    }
                }

                if (GUILayout.Button("Generate highlighted only"))
                {
                    RunPipeline(GetHighlightedTables(), validateOnly: false);
                }

                EditorGUILayout.HelpBox(
                    "Use table name toggles to build a subset, then generate here. Main \"Generate\" uses enabled checkboxes only.",
                    MessageType.None);
            }
        }

        private void DrawTableList()
        {
            if (_manifest == null)
            {
                EditorGUILayout.HelpBox("Manifest not loaded.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Manifest Tables", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("En", GUILayout.Width(24f));
                GUILayout.Label("Table", GUILayout.Width(200f));
                GUILayout.Label("Source", GUILayout.Width(60f));
                GUILayout.Label("Last Status", GUILayout.Width(120f));
                GUILayout.Label("Actions", GUILayout.Width(80f));
            }

            _tableScroll = EditorGUILayout.BeginScrollView(_tableScroll, GUILayout.Height(220f));
            foreach (var table in _manifest.Tables)
            {
                if (!string.IsNullOrEmpty(_search) &&
                    table.TableName.IndexOf(_search, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                var status = _lastTableStatus.TryGetValue(table.TableName, out var s) ? s : "Idle";
                if (_showOnlyFailed && !string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(status, "Missing", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    table.Enabled = EditorGUILayout.Toggle(table.Enabled, GUILayout.Width(20f));
                    var highlighted = _highlighted.Contains(table.TableName);
                    var nextHighlighted = GUILayout.Toggle(highlighted, table.TableName, "Button", GUILayout.Width(200f));
                    if (nextHighlighted != highlighted)
                    {
                        if (nextHighlighted)
                        {
                            _highlighted.Add(table.TableName);
                        }
                        else
                        {
                            _highlighted.Remove(table.TableName);
                        }
                    }

                    var source = _sourceKind.TryGetValue(table.TableName, out var sourceKind) ? sourceKind : "unknown";
                    GUILayout.Label(source, GUILayout.Width(60f));
                    GUILayout.Label(status, GUILayout.Width(120f));
                    if (GUILayout.Button("Generate", GUILayout.Width(75f)))
                    {
                        RunPipeline(new List<string> { table.TableName }, validateOnly: false);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.HelpBox(
                "Checkbox: include table in main Generate / Validate. Table name toggle: highlight for \"Selection & highlighted batch\".",
                MessageType.Info);
            EditorUtility.SetDirty(_manifest);
        }

        private void DrawAdvancedSettings()
        {
            if (_manifest == null)
            {
                return;
            }

            _showAdvanced = EditorGUILayout.Foldout(_showAdvanced, "Advanced (Global Row Layout)", true);
            if (!_showAdvanced)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _manifest.GlobalHeaderRowIndex = EditorGUILayout.IntField("Header Row (0-based)", _manifest.GlobalHeaderRowIndex);
                _manifest.GlobalTypeRowIndex = EditorGUILayout.IntField("Type Row (0-based)", _manifest.GlobalTypeRowIndex);
                _manifest.GlobalDataStartRowIndex = EditorGUILayout.IntField("Data Start Row (0-based)", _manifest.GlobalDataStartRowIndex);
                _manifest.GlobalAutoDetectTypeRowWhenEmpty = EditorGUILayout.Toggle("Auto Detect Type Row", _manifest.GlobalAutoDetectTypeRowWhenEmpty);
                EditorGUILayout.HelpBox("These defaults apply to all tables unless a table-level override is enabled in manifest asset.", MessageType.None);
                EditorUtility.SetDirty(_manifest);
            }
        }

        private List<string> GetHighlightedTables()
        {
            return _highlighted
                .OrderBy(x => x, System.StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private void LoadManifestAndSync()
        {
            _manifest = ConfigManifestService.LoadOrCreate();
            var parser = new ExcelParser();
            var discovered = parser.DiscoverAvailableTables();
            ConfigManifestService.SyncWithDiscovered(_manifest, discovered);
            _sourceKind.Clear();
            foreach (var t in _manifest.Tables)
            {
                _sourceKind[t.TableName] = parser.ResolveTableSourceKind(t.TableName);
            }

            // cleanup highlight items no longer in manifest
            var names = new HashSet<string>(_manifest.Tables.Select(t => t.TableName), StringComparer.OrdinalIgnoreCase);
            var dangling = _highlighted.Where(x => !names.Contains(x)).ToList();
            foreach (var d in dangling)
            {
                _highlighted.Remove(d);
            }
        }
    }
}
#endif
