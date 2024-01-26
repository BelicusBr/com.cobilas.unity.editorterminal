using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Cobilas.Unity.Editor.Terminal {
    public class UnityEditorTerminal : EditorWindow {

        private readonly static string[] terminals = {
            "CMD", "PowerShell", "MacOS Terminal", "GNOME Terminal"
        };

        [MenuItem("Window/UE-Terminal-Test")]
        private static void Init() {
            UnityEditorTerminal window = GetWindow<UnityEditorTerminal>();
            window.titleContent = new GUIContent("Unity Editor Terminal");
#if !UNITY_EDITOR_TERMINAL_TEST
            window.myTabs = new List<TerminalTab>();
            window.AddTerminal();
#endif
            window.Show();
        }

        private int currentTabIndex;
        private Vector2 scrollPosition;
        private List<TerminalTab> myTabs;
#if UNITY_EDITOR_TERMINAL_TEST
        private void OnEnable() {
            myTabs = new List<TerminalTab>();
            AddTerminal();
            CommandClippingMap clippingMap = new CommandClippingMap(
                "/uet -s -wd \"C:\\Path1\" > echo olá mundo!!!"
            );
        }
#endif
        private void OnGUI() {
            EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+", GUILayout.Width(25f)))
                    AddTerminal();
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, EditorStyles.toolbar);
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                        for (int I = 0; I < myTabs.Count; I++) {
                            if (ToolBarButton(myTabs[I].TabName)) {
                                currentTabIndex = myTabs.Count - 1;
                                titleContent = new GUIContent($"UET[{myTabs[I].TabName}]");
                            }
                            if (ToolBarButton("X")) {
                                RemoveTerminal(I);
                                break;
                            }
                            EditorGUILayout.Space(3f, false);
                        }
                        EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            if (GetCurretTerminalTab(out TerminalTab tab))
                tab.OnGUI();
        }

        private void RemoveTerminal(int index) {
            myTabs.RemoveAt(index);
            if (myTabs.Count == 0)
                titleContent = new GUIContent("Unity Editor Terminal");
        }

        private void AddTerminal() {
            myTabs.Add(new TerminalTab("new tab", Path.GetDirectoryName(Application.dataPath)));
            currentTabIndex = myTabs.Count - 1;
            titleContent = new GUIContent($"UET[{myTabs[currentTabIndex].TabName}]");
        }

        private bool GetCurretTerminalTab(out TerminalTab tab) {
            if (myTabs.Count == 0) {
                tab = (TerminalTab)null;
                return false;
            }
            if (currentTabIndex > myTabs.Count - 1)
                currentTabIndex = myTabs.Count - 1;
            tab = myTabs[currentTabIndex];
            return true;
        }

        private bool ToolBarButton(string text) {
            GUIContent txt = EditorGUIUtility.TrTempContent(text);
            GUIStyle style = EditorStyles.toolbarButton;
            return GUILayout.Button(txt, style, GUILayout.Width(style.CalcSize(txt).x));
        }
    }
}
