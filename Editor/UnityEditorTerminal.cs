using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Cobilas.Unity.Editor.Terminal {
    public class UnityEditorTerminal : EditorWindow {

        private static TerminalTask terminalTask = new TerminalTask();

        private readonly static string[] terminals = {
            "CMD", "PowerShell", "MacOS Terminal", "GNOME Terminal"
        };

        [MenuItem("Window/UE-Terminal")]
        private static void Init() {
            UnityEditorTerminal window = GetWindow<UnityEditorTerminal>();
            window.WorkingDirectory = Path.GetDirectoryName(Application.dataPath);
            window.titleContent = new GUIContent("Unity Editor Terminal");
            window.Show();
        }

        [InitializeOnLoadMethod]
        private static void Init_Funcs() {
            EditorApplication.update += terminalTask.Update;
        }

        private string saida;
        private string saida2;
        private GUIStyle style;
        private Rect scrollViewRect;
        [SerializeField] private float value;
        [SerializeField] private int selectTerminal;
        private Vector2 scrollPosition = Vector2.zero;
        [SerializeField] private float terminal_Height;
        private TextEditor textEditor = new TextEditor();
        [SerializeField] private string WorkingDirectory;
        private GUIContent contentTemp = new GUIContent();
        [SerializeField] private bool setWorkingDirectory;

        private void OnEnable() {
            style = null;
            saida2 = (string)(saida = ">").Clone();
        }

        private void OnGUI() {
            Event @event = Event.current;
            if (style is null) {
                style = new GUIStyle(GUI.skin.box);
                style.alignment = TextAnchor.UpperLeft;
            }

            contentTemp.text = saida2;

            EditorGUILayout.BeginHorizontal();
            if (setWorkingDirectory = GUILayout.Toggle(setWorkingDirectory, "Set", GUI.skin.button, GUILayout.Width(50f))) {
                WorkingDirectory = EditorGUILayout.TextField("WorkingDirectory", WorkingDirectory);
                if (string.IsNullOrEmpty(WorkingDirectory))
                    WorkingDirectory = Path.GetDirectoryName(Application.dataPath);
            } else EditorGUILayout.LabelField($"WorkingDirectory: {WorkingDirectory}", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            selectTerminal = EditorGUILayout.Popup("Terminals", selectTerminal, terminals);


            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(terminal_Height));
            
            if (@event.type == EventType.Repaint) {
                Vector2 textSize = style.CalcSize(contentTemp);
                terminal_Height = textSize.y;// < scrollViewRect.height ? scrollViewRect.height -4f : textSize.y;
            }

            if (@event.type == EventType.KeyDown)
                if (@event.keyCode == KeyCode.Return) {
                    terminalTask.terminal = this;
                    terminalTask.OpenTerminal();
                    @event.Use();
                }

            bool oldEnabled = GUI.enabled;
            GUI.enabled = terminalTask.IsCompleted;
            saida2 = DrawTextArea(rect, saida2, saida, @event, textEditor, style, GUIUtility.GetControlID(FocusType.Keyboard));
            GUI.enabled = oldEnabled;
            EditorGUILayout.EndScrollView();
            if (@event.type == EventType.Repaint)
                scrollViewRect = GUILayoutUtility.GetLastRect();
        }

        private string DrawTextArea(Rect rect, string text, string defaulttext, Event @event, TextEditor textEditor, GUIStyle style, int ID) {
            textEditor.text = text;
            textEditor.SaveBackup();
            textEditor.controlID = ID;
            textEditor.position = rect;
            textEditor.style = style;
            textEditor.multiline = true;
            textEditor.isPasswordField = false;
            textEditor.DetectFocusChange();

            bool isHover = rect.Contains(@event.mousePosition);

            switch (@event.type)
            {
                case EventType.MouseUp:
                    if (!isHover)
                    {
                        //isFocused = false;
                        GUIUtility.keyboardControl = 0;
                        textEditor.OnLostFocus();
                    }
                    if (GUIUtility.hotControl == ID)
                    {
                        textEditor.MouseDragSelectsWholeWords(false);
                        GUIUtility.hotControl = 0;
                        @event.Use();
                    }
                    break;
                case EventType.MouseDown:
                    if (!isHover) break;
                    GUIUtility.hotControl =
                        GUIUtility.keyboardControl = ID;
                    if (GUIUtility.keyboardControl == ID)
                    {
                        //isFocused = true;
                        textEditor.OnFocus();
                    }
                    textEditor.MoveCursorToPosition(@event.mousePosition);
                    if (@event.clickCount == 2 && GUI.skin.settings.doubleClickSelectsWord)
                    {
                        textEditor.SelectCurrentWord();
                        textEditor.DblClickSnap(TextEditor.DblClickSnapping.WORDS);
                        textEditor.MouseDragSelectsWholeWords(true);
                    }
                    if (@event.clickCount == 3 && GUI.skin.settings.tripleClickSelectsLine)
                    {
                        textEditor.SelectCurrentParagraph();
                        textEditor.MouseDragSelectsWholeWords(true);
                        textEditor.DblClickSnap(TextEditor.DblClickSnapping.PARAGRAPHS);
                    }
                    @event.Use();
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == ID)
                    {
                        if (@event.shift) textEditor.MoveCursorToPosition(@event.mousePosition);
                        else textEditor.SelectToPosition(@event.mousePosition);
                        @event.Use();
                    }
                    break;
                case EventType.ValidateCommand:
                case EventType.ExecuteCommand:
                    if (GUIUtility.keyboardControl == ID)
                    {
                        if (@event.commandName == "Copy")
                        {
                            textEditor.Copy();
                            @event.Use();
                        }
                        else if (@event.commandName == "Paste")
                        {
                            textEditor.Paste();
                            @event.Use();
                        }
                    }
                    break;
                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl != ID) break;
                    if (textEditor.HandleKeyEvent(@event))
                    {
                        @event.Use();
                        break;
                    }
                    char character = @event.character;
                    if (@event.keyCode == KeyCode.Tab || character == '\t')
                        break;
                    if (character == '\n' && !@event.alt)
                        break;
                    Font font = textEditor.style.font;
                    font = !font ? GUI.skin.font : font;

                    if (font.HasCharacter(character) || character == '\n')
                    {
                        textEditor.Insert(character);
                        break;
                    }
                    //if (character == char.MinValue) {
                    //    textEditor.ReplaceSelection("");
                    //    flag = true;
                    //    @event.Use();
                    //    break;
                    //}

                    switch (@event.keyCode)
                    {
                        case KeyCode.UpArrow:
                            if (@event.shift) textEditor.SelectUp();
                            else textEditor.MoveUp();
                            break;
                        case KeyCode.DownArrow:
                            if (@event.shift) textEditor.SelectDown();
                            else textEditor.MoveDown();
                            break;
                        case KeyCode.LeftArrow:
                            if (@event.shift) textEditor.SelectLeft();
                            else textEditor.MoveLeft();
                            break;
                        case KeyCode.RightArrow:
                            if (@event.shift) textEditor.SelectRight();
                            else textEditor.MoveRight();
                            break;
                        case KeyCode.Home:
                            if (@event.shift) textEditor.SelectGraphicalLineStart();
                            else textEditor.MoveGraphicalLineStart();
                            break;
                        case KeyCode.End:
                            if (@event.shift) textEditor.SelectGraphicalLineEnd();
                            else textEditor.MoveGraphicalLineEnd();
                            break;
                        case KeyCode.PageUp:
                            if (@event.shift) textEditor.SelectTextStart();
                            else textEditor.MoveTextStart();
                            break;
                        case KeyCode.PageDown:
                            if (@event.shift) textEditor.SelectTextEnd();
                            else textEditor.MoveTextEnd();
                            break;
                    }
                    @event.Use();
                    break;
                case EventType.Repaint:
                    if (GUIUtility.keyboardControl != ID)
                        textEditor.style.Draw(rect, EditorGUIUtility.TrTextContent(textEditor.text), ID);
                    else textEditor.DrawCursor(textEditor.text);
                    break;
            }

            if (textEditor.cursorIndex < defaulttext.Length)
                textEditor.cursorIndex = defaulttext.Length;
            if (textEditor.selectIndex < defaulttext.Length)
                textEditor.selectIndex = defaulttext.Length;
            if (textEditor.text.Length <= defaulttext.Length)
                return defaulttext;

            UnityEngine.Debug.Log($"{textEditor.cursorIndex}|{textEditor.selectIndex}|{defaulttext.Length}");

            textEditor.UpdateScrollOffsetIfNeeded(@event);
            return textEditor.text;
        }

        private sealed class TerminalTask {
            public int timer;
            public Task myTask;
            public Process process;
            public StringBuilder builder;
            public UnityEditorTerminal terminal;

            public bool IsCompleted => myTask is null;

            public void OpenTerminal() {
                builder = new StringBuilder();
                switch (terminal.selectTerminal) {
                    case 0:
                        //process.StartInfo = new ProcessStartInfo("cmd.exe", $"/c {arg}");
                        myTask = Task.Run(() => IOpenTerminal("cmd.exe", $"/c {terminal.saida2.Replace(terminal.saida, string.Empty)}", builder, terminal.WorkingDirectory));
                        break;
                    case 1:
                        //process.StartInfo = new ProcessStartInfo("powershell.exe", arg);
                        myTask = Task.Run(() => IOpenTerminal("powershell.exe", terminal.saida2.Replace(terminal.saida, string.Empty), builder, terminal.WorkingDirectory));
                        break;
                    case 2:
                        //process.StartInfo = new ProcessStartInfo("open", $"-a Terminal {arg}");
                        myTask = Task.Run(() => IOpenTerminal("open", $"-a Terminal  {terminal.saida2.Replace(terminal.saida, string.Empty)}", builder, terminal.WorkingDirectory));
                        break;
                    case 3:
                        //process.StartInfo = new ProcessStartInfo("gnome-terminal", arg);
                        myTask = Task.Run(() => IOpenTerminal("gnome-terminal", terminal.saida2.Replace(terminal.saida, string.Empty), builder, terminal.WorkingDirectory));
                        break;
                }
            }

            private void IOpenTerminal(string fileName, string arg, StringBuilder builder, string wd) {
                try {
                    process = new Process();
                    process.StartInfo = new ProcessStartInfo(fileName, arg);
                    process.StartInfo.WorkingDirectory = wd;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput =
                        process.StartInfo.RedirectStandardError =
                        process.StartInfo.CreateNoWindow = true;

                    process.OutputDataReceived += (arg1, arg2) => {
                        if (!string.IsNullOrEmpty(arg2.Data))
                            builder.AppendLine(arg2.Data);
                    };
                    process.ErrorDataReceived += (arg1, arg2) => {
                        if (!string.IsNullOrEmpty(arg2.Data))
                            builder.AppendLine(arg2.Data);
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();

                    process.Close();
                } catch (System.Exception e) {
                    UnityEngine.Debug.LogException(e);
                }
            }

            public void Update() {
                if (myTask is null) return;
                if (myTask.IsCompleted || myTask.IsFaulted) {
                    terminal.Repaint();
                    process.Dispose();
                    process = null;
                    myTask.Dispose();
                    myTask = null;
                    terminal.saida2 = (string)(terminal.saida = builder.ToString() + ">").Clone();
                    return;
                }
                if (++timer == 30) {
                    timer = 0;
                    terminal.saida2 = builder.ToString();
                    terminal.Repaint();
                }
            }
        }
    }
}
