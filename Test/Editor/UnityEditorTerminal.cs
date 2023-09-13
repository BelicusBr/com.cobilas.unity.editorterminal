using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Cobilas.Unity.Editor.Terminal {
    public class UnityEditorTerminal : EditorWindow {

        //private static TerminalTask terminalTask = new TerminalTask();

        private readonly static string[] terminals = {
            "CMD", "PowerShell", "MacOS Terminal", "GNOME Terminal"
        };

        [MenuItem("Window/UE-Terminal")]
        private static void Init() {
            UnityEditorTerminal window = GetWindow<UnityEditorTerminal>();
            //window.WorkingDirectory = Path.GetDirectoryName(Application.dataPath);
            window.titleContent = new GUIContent("Unity Editor Terminal");
            window.Show();
        }

        [InitializeOnLoadMethod]
        private static void Init_Funcs() {
            
        }

        private TerminalTab myTab;
        private Vector2 scrollPosition;

        private void OnEnable() {
            myTab = new TerminalTab("new tab", "");
        }

        private void OnGUI() {
            EditorGUILayout.BeginHorizontal();
                _ = GUILayout.Button("+", GUILayout.Width(25f));
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, EditorStyles.toolbar);
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                        _ = ToolBarButton("tab1");
                        _ = ToolBarButton("tab2");
                        _ = ToolBarButton("tab3");
                        EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            myTab.OnGUI();
        }

        private bool ToolBarButton(string text) {
            GUIContent txt = EditorGUIUtility.TrTempContent(text);
            GUIStyle style = EditorStyles.toolbarButton;
            return GUILayout.Button(txt, style, GUILayout.Width(style.CalcSize(txt).x));
        }
    }
}
