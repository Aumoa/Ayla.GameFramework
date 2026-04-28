#nullable enable

using System;
using System.Collections.Generic;
using UnityEditor;

namespace Ayla
{
    [CustomEditor(typeof(TimerMixerComponent), true)]
    public class TimerMixerComponentEditor : Editor
    {
        private TimerMixer? m_Mixer;

        private SerializedProperty m_Channel = null!;

        protected virtual void OnEnable()
        {
            m_Mixer = TimerMixer.Instance;
            m_Channel = serializedObject.FindProperty("m_Channel")
                ?? throw new InvalidOperationException("Failed to find serialized property 'm_Channel'.");
        }

        public override void OnInspectorGUI()
        {
            if (m_Mixer == null)
            {
                m_Mixer = TimerMixer.Instance;
            }

            serializedObject.Update();

            var scriptProp = serializedObject.FindProperty("m_Script");
            if (scriptProp != null)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(scriptProp);
                }
            }

            if (m_Mixer == null)
            {
                EditorGUILayout.HelpBox(TimerMixerText.NotFoundLabel, MessageType.Warning);
            }
            else
            {
                var channels = new List<TimerChannel?> { null };
                var displayNames = new List<string> { TimerMixerText.MasterLabel };
                CollectChannels(m_Mixer, string.Empty, channels, displayNames);

                var currentChannel = m_Channel.objectReferenceValue as TimerChannel;
                int currentIndex = channels.IndexOf(currentChannel);
                if (currentIndex < 0)
                {
                    currentIndex = 0;
                }

                int newIndex = EditorGUILayout.Popup(TimerMixerText.ChannelHeader, currentIndex, displayNames.ToArray());
                if (newIndex != currentIndex)
                {
                    m_Channel.objectReferenceValue = channels[newIndex];
                }

                ITimerChannel selectedChannel = channels[currentIndex] ?? (ITimerChannel)m_Mixer;
                DrawChannelInfo(selectedChannel);
            }

            DrawPropertiesExcluding(serializedObject, "m_Script", "m_Channel");
            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawChannelInfo(ITimerChannel channel)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.FloatField(TimerMixerText.SelfTimeScaleLabel, (float)channel.SelfTimeScale);
                EditorGUILayout.FloatField(TimerMixerText.FinalTimeScaleLabel, (float)channel.TimeScale);
            }
            EditorGUILayout.EndVertical();
        }

        private static void CollectChannels(ITimerChannel parent, string parentPath, List<TimerChannel?> channels, List<string> displayNames)
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
