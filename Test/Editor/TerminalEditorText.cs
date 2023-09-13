using System;
using UnityEditor;
using UnityEngine;

namespace Cobilas.Unity.Editor.Terminal {
    public sealed class TerminalEditorText {
        
        public event Action<string> GetCommand;

        private int textLength;
        private bool getTextLength;
        private TextEditor textEditor;
        private float cursorFlashSpeed;

        public string Text { 
            get => textEditor.text;
            set => GetText(value);
        }
        
        public TerminalEditorText() {
            textLength = 0;
            textEditor = new TextEditor();
            textEditor.isPasswordField = 
                getTextLength = false;
            textEditor.multiline = true;
        }

        public void OnGUI(Rect rect, GUIStyle style) {
            Event @event = Event.current;
            textEditor.SaveBackup();
            textEditor.style = style;
            textEditor.position = rect;
            textEditor.controlID = GUIUtility.GetControlID(FocusType.Keyboard);
            textEditor.DetectFocusChange();

            MouseChange(@event);
            CopyPaste(@event);
            KeyboardChange(@event);
            Repaint(@event);

            textEditor.UpdateScrollOffsetIfNeeded(@event);
        }

        public void Reset() => getTextLength = false;

        private void GetText(string txt) {
            textEditor.text = txt;
            if (!getTextLength) {
                getTextLength = true;
                textEditor.cursorIndex =
                    textEditor.selectIndex =
                         textLength = txt.Length;
            }
        }

        private void MouseChange(Event @event) {
            bool isHover = textEditor.position.Contains(@event.mousePosition);

            switch (@event.type) {
                case EventType.MouseDown:
                    if (!isHover) break;
                    GUIUtility.hotControl =
                        GUIUtility.keyboardControl = textEditor.controlID;
                    if (GUIUtility.keyboardControl == textEditor.controlID)
                        textEditor.OnFocus();
                    textEditor.MoveCursorToPosition(@event.mousePosition);
                    if (@event.clickCount == 2 && GUI.skin.settings.doubleClickSelectsWord) {
                        textEditor.SelectCurrentWord();
                        textEditor.DblClickSnap(TextEditor.DblClickSnapping.WORDS);
                        textEditor.MouseDragSelectsWholeWords(true);
                    }
                    if (@event.clickCount == 3 && GUI.skin.settings.tripleClickSelectsLine) {
                        textEditor.SelectCurrentParagraph();
                        textEditor.MouseDragSelectsWholeWords(true);
                        textEditor.DblClickSnap(TextEditor.DblClickSnapping.PARAGRAPHS);
                    }
                    @event.Use();
                    break;
                case EventType.MouseUp:
                    if (!isHover) {
                        GUIUtility.keyboardControl = 0;
                        textEditor.OnLostFocus();
                    }
                    if (GUIUtility.hotControl == textEditor.controlID) {
                        textEditor.MouseDragSelectsWholeWords(false);
                        GUIUtility.hotControl = 0;
                        @event.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == textEditor.controlID) {
                        if (@event.shift) textEditor.MoveCursorToPosition(@event.mousePosition);
                        else textEditor.SelectToPosition(@event.mousePosition);
                        @event.Use();
                    }
                    break;
            }
        }
    
        private void CopyPaste(Event @event) {
            switch (@event.type) {
                case EventType.ValidateCommand:
                case EventType.ExecuteCommand:
                    if (GUIUtility.keyboardControl == textEditor.controlID) {
                        if (@event.commandName == "Copy") {
                            textEditor.Copy();
                            @event.Use();
                        } else if (@event.commandName == "Paste") {
                            GUI.changed = true;
                            textEditor.Paste();
                            @event.Use();
                        }
                    }
                    break;
            }
        }
    
        private void KeyboardChange(Event @event) {
            switch (@event.type) {
                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl != textEditor.controlID) break;
                    if (@event.keyCode == KeyCode.Backspace) {
                        PositionCursorCorrectly(false);
                        if (textEditor.selectIndex == textLength &&
                            textEditor.cursorIndex == textLength)
                                break;
                        GUI.changed = true;
                    }
                    if (textEditor.HandleKeyEvent(@event)) {
                        @event.Use();
                        break;
                    }
                    char character = @event.character;

                    if (@event.keyCode == KeyCode.Tab || character == '\t')
                        break;

                    Font font = textEditor.style.font;
                    font = !font ? GUI.skin.font : font;

                    switch (@event.keyCode) {
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
                        case KeyCode.Return:
                        case KeyCode.KeypadEnter:
                            GetCommand?.Invoke(textEditor.text.Remove(0, textLength));
                            break;
                        default:
                            if (font.HasCharacter(character)) {
                                PositionCursorCorrectly(true);
                                GUI.changed = true;
                                textEditor.Insert(character);
                            }
                            break;
                    }
                    @event.Use();
                    break;
            }
        }
    
        private void Repaint(Event @event) {
            switch (@event.type) {
                case EventType.Repaint:
                    if (GUIUtility.keyboardControl != textEditor.controlID)
                        textEditor.style.Draw(textEditor.position, EditorGUIUtility.TrTextContent(textEditor.text), textEditor.controlID);
                    else textEditor.DrawCursor(textEditor.text);
                    break;
            }
        }
    
        private void PositionCursorCorrectly(bool ForTheEnd) {
            bool n_selectIndex = textEditor.selectIndex < textLength;
            bool n_cursorIndex = textEditor.cursorIndex < textLength;
            textEditor.selectIndex = n_selectIndex ? ForTheEnd ? textEditor.text.Length : textLength : textEditor.selectIndex;
            textEditor.cursorIndex = n_cursorIndex ? ForTheEnd ? textEditor.text.Length : textLength : textEditor.cursorIndex;
        }
    }
}