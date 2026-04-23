#if UNITY_EDITOR
using Game.Config.Editor.Generator;
using UnityEditor;
using UnityEngine;

namespace Game.Config.Editor.Window
{
    /// <summary>Unity 菜单打开的生成窗口：触发管线并展示校验错误列表。</summary>
    public sealed class ConfigGenerationWindow : EditorWindow
    {
        private string _status = "Idle";
        private Vector2 _scroll;
        private string _errors = string.Empty;

        /// <summary>菜单入口：<c>Tools/Config/Generation Window</c>。</summary>
        [MenuItem("Tools/Config/Generation Window")]
        private static void Open()
        {
            var window = GetWindow<ConfigGenerationWindow>();
            window.titleContent = new GUIContent("Config Generator");
            window.Show();
        }

        /// <summary>绘制窗口 UI：按钮 + 状态 + 错误滚动区。</summary>
        private void OnGUI()
        {
            GUILayout.Label("Config Generation Pipeline", EditorStyles.boldLabel);
            if (GUILayout.Button("Run Phase-1 Generate"))
            {
                RunPipeline();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Status", _status);

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(240f));
            EditorGUILayout.TextArea(_errors);
            EditorGUILayout.EndScrollView();
        }

        /// <summary>调用 <see cref="ConfigGenerationPipeline.Run"/> 并根据结果更新界面文案。</summary>
        private void RunPipeline()
        {
            var pipeline = new ConfigGenerationPipeline();
            var report = pipeline.Run();
            if (report.IsValid)
            {
                _status = "Success";
                _errors = "No validation errors.";
            }
            else
            {
                _status = "Validation Failed";
                _errors = string.Join(
                    "\n",
                    report.Errors.ConvertAll(e => $"{e.FileName}/{e.SheetName} r{e.RowNumber} c{e.ColumnName}: {e.Message}"));
            }

            Repaint();
        }
    }
}
#endif
