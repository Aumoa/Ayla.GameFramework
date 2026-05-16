#nullable enable

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ayla
{
    [CustomPropertyDrawer(typeof(TimerChannel))]
    internal class TimerChannelReferencePropertyDrawer : PropertyDrawer
    {
        // Popup row + two read-only FloatFields for channel info
        private const int k_ChannelInfoLineCount = 2;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            return lineHeight * (1 + k_ChannelInfoLineCount);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var mixer = TimerMixer.Instance;
            if (mixer == null)
            {
                EditorGUI.HelpBox(position, TimerMixerText.NotFoundLabel, MessageType.Warning);
                return;
            }

            float lineHeight  = EditorGUIUtility.singleLineHeight;
            float lineSpacing = EditorGUIUtility.standardVerticalSpacing;
            float fullLine    = lineHeight + lineSpacing;

            var channels     = new List<TimerChannel?> { null };
            var displayNames = new List<string> { TimerMixerText.MasterLabel };
            CollectChannels(mixer, string.Empty, channels, displayNames);

            var currentChannel = property.objectReferenceValue as TimerChannel;
            int currentIndex   = channels.IndexOf(currentChannel);
            if (currentIndex < 0)
                currentIndex = 0;

            var popupRect = new Rect(position.x, position.y, position.width, lineHeight);
            var channelName = label.text.Equals("Channel", System.StringComparison.OrdinalIgnoreCase) ? TimerMixerText.ChannelHeader : label.text;

            int newIndex  = EditorGUI.Popup(popupRect, channelName, currentIndex, displayNames.ToArray());
            if (newIndex != currentIndex)
                property.objectReferenceValue = channels[newIndex];

            var boxRect = new Rect(position.x, position.y + fullLine, position.width, lineHeight * k_ChannelInfoLineCount + lineSpacing);
            DrawChannelInfo(boxRect, channels[newIndex < 0 ? currentIndex : newIndex] ?? (ITimerChannel)mixer, lineHeight, fullLine);
        }

        private static void DrawChannelInfo(Rect boxRect, ITimerChannel channel, float lineHeight, float fullLine)
        {
            GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);

            using (new EditorGUI.DisabledScope(true))
            {
                var selfRect  = new Rect(boxRect.x, boxRect.y, boxRect.width, lineHeight);
                var finalRect = new Rect(boxRect.x, boxRect.y + fullLine, boxRect.width, lineHeight);
                EditorGUI.FloatField(selfRect,  TimerMixerText.SelfTimeScaleLabel,  (float)channel.SelfTimeScale);
                EditorGUI.FloatField(finalRect, TimerMixerText.FinalTimeScaleLabel, (float)channel.TimeScale);
            }
        }

        internal static void CollectChannels(
            ITimerChannel parent,
            string parentPath,
            List<TimerChannel?> channels,
            List<string> displayNames)
        {
            foreach (var child in parent.Children)
            {
                if (child is TimerChannel tc)
                {
                    var path = string.IsNullOrEmpty(parentPath) ? tc.name : $"{parentPath}/{tc.name}";
                    channels.Add(tc);
                    displayNames.Add(path);
                    CollectChannels(tc, path, channels, displayNames);
                }
            }
        }
    }
}
