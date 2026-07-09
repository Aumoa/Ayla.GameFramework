#nullable enable

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using static Ayla.TimerMixerStylesheet;

namespace Ayla
{
    public class TimerMixerEditorWindow : EditorWindow
    {
        private TimerMixer m_TimerMixer = null!;
        private GUIContent m_AddChildContent = null!;
        private GUIContent m_RenameChannelContent = null!;
        private GUIContent m_DeleteChannelContent = null!;
        private ITimerChannel? m_SelectedChannel;
        private string? m_EditingName;
        private bool m_RequestFocusForRename;
        private readonly HashSet<EntityId> m_CollapsedChannels = new();
        private float m_GroupContentHeight;
        private double m_LastClickTime;
        private ITimerChannel? m_LastClickedChannel;

        [SerializeField]
        private float m_ScrollY;

        private void OnEnable()
        {
            m_AddChildContent = new GUIContent(TimerMixerText.AddChild);
            m_RenameChannelContent = new GUIContent(TimerMixerText.RenameChannel);
            m_DeleteChannelContent = new GUIContent(TimerMixerText.DeleteChannel);
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
                    m_TimerMixer = newAsset;
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

            HorizontalBorder.Draw(headerBorder, AssetReferenceStylesheet.BorderColor);
            VerticalBorder.Draw(leftBorder, AssetReferenceStylesheet.BorderColor);
        }

        private void OnLostFocus()
        {
            SaveNameAndExitEditMode();
        }

        private void DrawContentPanel(Rect rect)
        {
            EditorGUI.DrawRect(rect, AssetReferenceStylesheet.MixerContentPanelColor);

            float headerH = EditorGUIUtility.singleLineHeight + 4;
            DrawContentColumnHeaders(rect.FillTop(headerH));
            EditorGUI.DrawRect(rect.MarginTop(headerH).FillTop(1), AssetReferenceStylesheet.BorderColor);

            var contentArea = rect.MarginTop(headerH + 1);
            var contentHeight = Mathf.Max(contentArea.height, m_GroupContentHeight);
            var viewRect = new Rect(0, 0, contentArea.width - 16f, contentHeight);
            var scrollPos = GUI.BeginScrollView(contentArea, new Vector2(0, m_ScrollY), viewRect, false, false);
            m_ScrollY = scrollPos.y;

            var evt = Event.current;
            DrawChannelSliderRow(evt, m_TimerMixer, 0,
                new Rect(0, 4, viewRect.width, viewRect.height), m_TimerMixer.TimeScale);

            GUI.EndScrollView();
        }

        private void DrawContentColumnHeaders(Rect rect)
        {
            EditorGUI.DrawRect(rect, ColumnHeaderColor);

            var contentRect = rect.Margin(4, 2);
            var finalRect = contentRect.FillRight(kContentFinalColWidth);
            var sliderHeaderRect = contentRect.MarginRight(kContentFinalColWidth + kContentColGap);

            GUI.Label(sliderHeaderRect, TimerMixerText.TimeScaleLabel, EditorStyles.miniLabel);
            GUI.Label(finalRect, TimerMixerText.FinalHeader, RightMiniLabelStyle);
        }

        private float DrawChannelSliderRow(Event evt, ITimerChannel channel, int indent, Rect drawRect, double finalTS)
        {
            float rowH = EditorGUIUtility.singleLineHeight + 4;
            var rowRect = drawRect.FillTop(rowH);
            float totalHeight = rowH;

            if (m_SelectedChannel == channel)
            {
                EditorGUI.DrawRect(rowRect, SelectedBackgroundColor);
            }

            var rowContent = rowRect.Margin(4, 2);
            GUI.Label(rowContent.FillRight(kContentFinalColWidth), $"{finalTS:F3}×", FinalTsStyle);

            var sliderRect = rowContent.MarginRight(kContentFinalColWidth + kContentColGap);

            if (channel is TimerMixer)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.Slider(sliderRect, (float)channel.SelfTimeScale, 0f, 3f);
                }
            }
            else
            {
                var so = new SerializedObject((Object)channel);
                so.Update();
                var prop = so.FindProperty("m_TimeScale");
                if (prop != null)
                {
                    EditorGUI.BeginChangeCheck();
                    float newVal = EditorGUI.Slider(sliderRect, (float)prop.doubleValue, 0f, 3f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        prop.doubleValue = newVal;
                    }
                }
                if (so.ApplyModifiedProperties())
                {
                    Repaint();
                }
            }

            if (evt.rawType == EventType.MouseDown && evt.button == 0 && rowRect.Contains(evt.mousePosition))
            {
                if (m_SelectedChannel != channel)
                {
                    SaveNameAndExitEditMode();
                    m_SelectedChannel = channel;
                    m_EditingName = null;
                    Repaint();
                }
            }

            bool hasChildren = channel.Children.Count > 0;
            bool isExpanded = hasChildren && !m_CollapsedChannels.Contains(((Object)channel).GetEntityId());
            drawRect = drawRect.MarginTop(rowH);

            if (isExpanded)
            {
                foreach (var child in channel.Children)
                {
                    float childH = DrawChannelSliderRow(evt, child, indent + 1, drawRect, finalTS * child.SelfTimeScale);
                    drawRect = drawRect.MarginTop(childH);
                    totalHeight += childH;
                }
            }

            return totalHeight;
        }

        private void SaveNameAndExitEditMode()
        {
            if (m_SelectedChannel != null && !string.IsNullOrEmpty(m_EditingName) && m_SelectedChannel.Name != m_EditingName)
            {
                Undo.RecordObject((Object)m_SelectedChannel, "Change Channel Name");
                m_SelectedChannel.Name = m_EditingName!;
                EditorUtility.SetDirty(m_TimerMixer);
            }
            m_EditingName = null;
        }

        private void DrawGroupPanel(Rect rect)
        {
            EditorGUI.DrawRect(rect, AssetReferenceStylesheet.MixerGroupPanelColor);
            var current = Event.current;

            if (m_SelectedChannel != null && m_EditingName != null && current.type == EventType.KeyDown)
            {
                if (current.keyCode == KeyCode.Escape)
                {
                    m_EditingName = null;
                    current.Use();
                    Repaint();
                }
                else if (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter)
                {
                    SaveNameAndExitEditMode();
                    current.Use();
                    Repaint();
                }
            }

            bool clickedInGroupArea = current.type == EventType.MouseDown && current.button == 0 && rect.Contains(current.mousePosition);

            float headerH = EditorGUIUtility.singleLineHeight + 4;
            EditorGUI.DrawRect(rect.FillTop(headerH), ColumnHeaderColor);
            EditorGUI.DrawRect(rect.MarginTop(headerH).FillTop(1), AssetReferenceStylesheet.BorderColor);

            float headerOffset = headerH + 1;
            var scrollArea = rect.MarginTop(headerOffset);
            var contentHeight = Mathf.Max(scrollArea.height, m_GroupContentHeight);
            var viewRect = new Rect(0, 0, scrollArea.width - 16f, contentHeight);
            var scrollPos = GUI.BeginScrollView(scrollArea, new Vector2(0, m_ScrollY), viewRect, false, false);
            m_ScrollY = scrollPos.y;

            float totalHeight = DrawChannelRecursive(current, m_TimerMixer, null,
                new Rect(0, 4, viewRect.width, viewRect.height), 0);
            if (current.type == EventType.Repaint)
            {
                m_GroupContentHeight = totalHeight + 4;
            }

            GUI.EndScrollView();

            if (clickedInGroupArea && current.type == EventType.MouseDown)
            {
                SaveNameAndExitEditMode();
                m_SelectedChannel = null;
                current.Use();
                Repaint();
            }

            return;

            float DrawChannelRecursive(Event evt, ITimerChannel channel, ITimerChannel? parent, Rect drawRect, int indent)
            {
                float rowH = EditorGUIUtility.singleLineHeight + 4;
                var rowRect = drawRect.FillTop(rowH);
                var contentRect = rowRect.MiddleVertical(EditorGUIUtility.singleLineHeight);
                var expanderRect = contentRect.FillLeft(EditorGUIUtility.singleLineHeight);
                var labelRect = contentRect.MarginLeft(expanderRect.width);
                var indentedLabelRect = labelRect.MarginLeft(indent * 15 + 4);
                float height = rowH;

                if (m_SelectedChannel == channel)
                {
                    EditorGUI.DrawRect(rowRect, SelectedBackgroundColor);
                }

                bool hasChildren = channel.Children.Count > 0;
                var entityId = ((Object)channel).GetEntityId();
                bool isExpanded = hasChildren && !m_CollapsedChannels.Contains(entityId);

                if (hasChildren)
                {
                    var iconName = isExpanded ? "d_IN_foldout_on" : "d_IN_foldout";
                    GUI.Label(expanderRect, EditorGUIUtility.IconContent(iconName));

                    if (evt.type == EventType.MouseDown && evt.button == 0 && expanderRect.Contains(evt.mousePosition))
                    {
                        if (isExpanded)
                        {
                            m_CollapsedChannels.Add(entityId);
                        }
                        else
                        {
                            m_CollapsedChannels.Remove(entityId);
                        }
                        evt.Use();
                        Repaint();
                    }
                }

                if (channel is TimerMixer)
                {
                    GUI.Label(indentedLabelRect, "Master", EditorStyles.boldLabel);
                }
                else if (channel == m_SelectedChannel && m_EditingName != null)
                {
                    GUI.SetNextControlName("RenameField");
                    m_EditingName = GUI.TextField(indentedLabelRect, m_EditingName);
                    if (m_RequestFocusForRename)
                    {
                        EditorGUI.FocusTextInControl("RenameField");
                        m_RequestFocusForRename = false;
                        var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                        if (editor != null)
                        {
                            editor.SelectAll();
                        }
                        Repaint();
                    }
                }
                else
                {
                    GUI.Label(indentedLabelRect, ((Object)channel).name);
                }

                drawRect = drawRect.MarginTop(height);

                if (evt.type == EventType.MouseDown && evt.button == 1 && labelRect.Contains(evt.mousePosition))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(m_AddChildContent, false, () =>
                    {
                        var newChannel = TimerMixerUtility.AddChild(channel, "New Channel");
                        m_SelectedChannel = newChannel;
                        m_EditingName = "New Channel";
                        m_RequestFocusForRename = true;
                        Repaint();
                    });
                    if (parent != null)
                    {
                        menu.AddSeparator(string.Empty);
                        var capturedChannel = channel;
                        var capturedParent = parent;
                        menu.AddItem(m_RenameChannelContent, false, () =>
                        {
                            m_SelectedChannel = capturedChannel;
                            m_EditingName = capturedChannel.Name;
                            m_RequestFocusForRename = true;
                            Repaint();
                        });
                        menu.AddItem(m_DeleteChannelContent, false, () =>
                        {
                            if (m_SelectedChannel == capturedChannel)
                            {
                                m_SelectedChannel = null;
                                m_EditingName = null;
                            }
                            TimerMixerUtility.RemoveChannel(capturedParent, capturedChannel);
                            Repaint();
                        });
                    }

                    menu.ShowAsContext();
                    evt.Use();
                }

                if (isExpanded)
                {
                    foreach (var child in channel.Children)
                    {
                        float childHeight = DrawChannelRecursive(evt, child, channel, drawRect, indent + 1);
                        drawRect = drawRect.MarginTop(childHeight);
                        height += childHeight;
                    }
                }

                if (evt.type == EventType.MouseDown && evt.button == 0 && labelRect.Contains(evt.mousePosition))
                {
                    if (channel is not TimerMixer)
                    {
                        double now = EditorApplication.timeSinceStartup;
                        bool isDoubleClick = m_SelectedChannel == channel && m_LastClickedChannel == channel &&
                                             now - m_LastClickTime < 0.3;
                        m_LastClickTime = now;
                        m_LastClickedChannel = channel;

                        if (isDoubleClick)
                        {
                            m_EditingName = channel.Name;
                            m_RequestFocusForRename = true;
                        }
                        else
                        {
                            SaveNameAndExitEditMode();
                            m_SelectedChannel = channel;
                            m_EditingName = null;
                        }
                    }
                    else
                    {
                        SaveNameAndExitEditMode();
                        m_SelectedChannel = channel;
                        m_EditingName = null;
                    }
                    evt.Use();
                    Repaint();
                }

                return height;
            }
        }

        private void DrawToolbar(Rect rect)
        {
            EditorGUI.DrawRect(rect, AssetReferenceStylesheet.ToolbarColor);
            var labelRect = rect.MarginLeft(8).MiddleVertical(EditorGUIUtility.singleLineHeight);
            GUI.Label(labelRect, m_TimerMixer != null ? m_TimerMixer.name : "Timer Mixer", EditorStyles.boldLabel);
        }

        [MenuItem("Window/Ayla/Timer Mixer")]
        public static void OpenWindow()
        {
            GetWindow<TimerMixerEditorWindow>();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(EntityId entityId)
        {
            if (EditorUtility.EntityIdToObject(entityId) is TimerMixer)
            {
                OpenWindow();
                return true;
            }
            return false;
        }
    }
}
