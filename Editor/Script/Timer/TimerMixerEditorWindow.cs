#nullable enable

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Ayla
{
    public class TimerMixerEditorWindow : EditorWindow
    {
        private TimerMixer m_TimerMixer;
        private GUIContent m_AddChildContent;
        private ITimerChannel? m_SelectedChannel;
        private string? m_EditingName;
        private bool m_RequestFocusForRename;

        [SerializeField]
        private float m_ScrollY;

        private void OnEnable()
        {
            m_AddChildContent = new GUIContent(TimerMixerText.AddChild);
            m_TimerMixer = AssetDatabase.LoadAssetAtPath<TimerMixer>(TimerMixer.kDefaultAssetPath);
        }

        private void OnGUI()
        {
            if (m_TimerMixer == null)
            {
                GUILayout.Label(TimerMixerText.NoAssetText);
                if (GUILayout.Button(TimerMixerText.Create))
                {
                    var newAsset = CreateInstance<TimerMixer>();
                    AssetDatabase.CreateAsset(newAsset, TimerMixer.kDefaultAssetPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                return;
            }

            var drawingRect = new Rect(Vector2.zero, position.size);
            var toolbarRect = drawingRect.FillTop(20);
            DrawToolbar(toolbarRect);
            drawingRect = drawingRect.MarginTop(toolbarRect.height);

            var headerBorder = new DrawingArgs(drawingRect, drawingRect);
            drawingRect = drawingRect.MarginTop(1);

            var groupRect = drawingRect.FillLeft(200);
            DrawGroupPanel(groupRect);
            drawingRect = drawingRect.MarginLeft(groupRect.width);

            var leftBorder = new DrawingArgs(drawingRect, drawingRect);
            drawingRect = drawingRect.MarginLeft(1);

            DrawContentPanel(drawingRect);

            HorizontalBorder.Draw(headerBorder, Stylesheet.BorderColor);
            VerticalBorder.Draw(leftBorder, Stylesheet.BorderColor);
        }

        private void DrawContentPanel(Rect rect)
        {
            EditorGUI.DrawRect(rect, Stylesheet.MixerContentPanelColor);
        }

        private void SaveNameAndExitEditMode()
        {
            if (m_SelectedChannel != null && string.IsNullOrEmpty(m_EditingName) == false && m_SelectedChannel.Name != m_EditingName)
            {
                Undo.RecordObject((Object)m_SelectedChannel, "Change Channel Name");
                m_SelectedChannel.Name = m_EditingName!;
                EditorUtility.SetDirty(m_TimerMixer);
                m_EditingName = null;
                Repaint();
            }
        }

        private void DrawGroupPanel(Rect rect)
        {
            EditorGUI.DrawRect(rect, Stylesheet.MixerGroupPanelColor);
            var current = Event.current;

            if (m_SelectedChannel != null && string.IsNullOrEmpty(m_EditingName) == false && current.rawType == EventType.KeyDown)
            {
                if (current.keyCode == KeyCode.Escape)
                {
                    m_EditingName = null;
                    current.Use();
                    Repaint();
                }
                else if (current.keyCode == KeyCode.Return)
                {
                    SaveNameAndExitEditMode();
                    current.Use();
                    Repaint();
                }
            }

            using (GUIScope.Area(rect))
            {
                var localRect = rect.ZeroPosition();
                DrawChannelRecursive(current, m_TimerMixer, localRect, 0);
            }

            if (current.rawType == EventType.MouseDown && current.button == 0)
            {
                SaveNameAndExitEditMode();
                m_SelectedChannel = null;
                current.Use();
                Repaint();
            }

            return;

            float DrawChannelRecursive(Event current, ITimerChannel channel, Rect rect, int indent)
            {
                var currentRect = rect.FillTop(EditorGUIUtility.singleLineHeight);
                var expanderRect = currentRect.FillLeft(EditorGUIUtility.singleLineHeight);
                GUI.Toggle(expanderRect, false, EditorGUIUtility.IconContent("stylesheets/northstar/images/d_dropdown@2x.png"), EditorStyles.label);
                var labelRect = currentRect.MarginLeft(expanderRect.width);
                var indentedLabelRect = labelRect.MarginLeft(indent * 15);
                float height = currentRect.height;

                if (m_SelectedChannel == channel)
                {
                    EditorGUI.DrawRect(currentRect, Color.aliceBlue.WithAlpha(0.4f));
                }

                if (channel is TimerMixer)
                {
                    GUI.Label(indentedLabelRect, "Master");
                }
                else
                {
                    if (channel == m_SelectedChannel && m_EditingName != null)
                    {
                        GUI.SetNextControlName("RenameField");
                        m_EditingName = EditorGUI.TextField(indentedLabelRect, m_EditingName);
                        if (m_RequestFocusForRename && GUI.GetNameOfFocusedControl() == "RenameField")
                        {
                            GUI.FocusControl("RenameField");
                            var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                            if (editor != null)
                            {
                                editor.SelectAll();
                            }
                            m_RequestFocusForRename = false;
                        }
                    }
                    else
                    {
                        GUI.Label(indentedLabelRect, ((Object)channel).name);
                    }
                }
                rect = rect.MarginTop(currentRect.height + EditorGUIUtility.standardVerticalSpacing);

                if (current.rawType == EventType.MouseDown && current.button == 1 && labelRect.Contains(current.mousePosition))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(m_AddChildContent, false, () =>
                    {
                        var newChannelAsset = TimerMixerUtility.AddChild(channel, "New Channel");
                        m_SelectedChannel = newChannelAsset;
                        m_EditingName = "New Channel";
                        m_RequestFocusForRename = true;
                    });
                    menu.ShowAsContext();
                }

                foreach (var child in channel.Children)
                {
                    var currentChildHeight = DrawChannelRecursive(current, child, rect, indent + 1);
                    rect = rect.MarginTop(currentChildHeight);
                    height += currentChildHeight;
                }

                if (current.rawType == EventType.MouseDown && current.button == 0 && labelRect.Contains(current.mousePosition))
                {
                    m_SelectedChannel = channel;
                    m_EditingName = null;
                    current.Use();
                    Repaint();
                }

                if (m_SelectedChannel != null && current.rawType == EventType.KeyDown && current.keyCode == KeyCode.F2)
                {
                    m_EditingName = m_SelectedChannel.Name;
                    m_RequestFocusForRename = true;
                    current.Use();
                    Repaint();
                }

                return height;
            }
        }

        private void DrawToolbar(Rect rect)
        {
            EditorGUI.DrawRect(rect, Stylesheet.ToolbarColor);
        }

        [MenuItem("Window/Ayla/Timer Mixer")]
        public static void OpenWindow()
        {
            GetWindow<TimerMixerEditorWindow>();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId)
        {
            if (EditorUtility.EntityIdToObject(instanceId) is TimerMixer)
            {
                OpenWindow();
                return true;
            }

            return false;
        }
    }
}
