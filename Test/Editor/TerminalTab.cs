using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Cobilas.Unity.Editor.Terminal {
    [Serializable]
    public sealed class TerminalTab : IDisposable {
        private string text;
        private bool disposedValue;
        private bool applayCommand;
        private GUIStyle style = null;
        private Vector2 scrollPosition;
        private GUILayoutOption option = null;
        private TerminalEditorText editorText;
        [SerializeField] private string tabName;
        [SerializeField] private string workingDirectory;

        public string TabName => tabName;

        public TerminalTab(string tabName, string workingDirectory) {
            text = ">";
            applayCommand = false;
            this.tabName = tabName;
            this.workingDirectory = workingDirectory;
            editorText = new TerminalEditorText();
            editorText.GetCommand += GetCommand;
        }

        public void OnGUI() {
            if (style is null) {
                style = new GUIStyle(EditorStyles.helpBox);
                style.alignment = TextAnchor.UpperLeft;
                style.fontSize = 12;
            }
            if (option is null)
                option = GUILayout.ExpandHeight(true);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            Rect rect1 = EditorGUILayout.GetControlRect(option);
            //Debug.Log($"rect1:{rect1.position}|mpos:{Event.current.mousePosition}");

            editorText.Text = text;
            editorText.OnGUI(rect1, style);
            EditorGUILayout.EndScrollView();

            Vector2 textSize = style.CalcSize(EditorGUIUtility.TrTempContent(text));
            Rect sv_rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint) {
                if (textSize.y > sv_rect.height)
                    option = GUILayout.Height(textSize.y);
                else option = GUILayout.ExpandHeight(true);
            }

            if (!applayCommand)
                text = editorText.Text;
            else applayCommand = false;
        }

        public void Dispose() {
            // Não altere este código. Coloque o código de limpeza no método 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void GetCommand(string txt) {
            //if (string.IsNullOrEmpty(txt)) return;
            applayCommand = true;
            StringBuilder builder = new StringBuilder(text);
            builder.AppendLine();
            builder.AppendLine(txt);
            builder.Append(">");
            text = builder.ToString();
            editorText.Reset();
            Vector2 textSize = style.CalcSize(EditorGUIUtility.TrTempContent(text));
            scrollPosition.y = textSize.y;
            Debug.Log(txt);
        }

        private void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                }

                disposedValue = true;
            }
        }
    }
}