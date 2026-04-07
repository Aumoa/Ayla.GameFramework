#nullable enable

using UnityEditor;
using UnityEngine;

namespace Ayla
{
    internal static class TimerMixerRegister
    {
        private static Editor? s_CachedEditor;

        [SettingsProvider]
        public static SettingsProvider CreateTimerMixerProvider()
        {
            return new SettingsProvider("Project/TimerMixer", SettingsScope.Project)
            {
                label = "Timer Mixer",
                guiHandler = _ =>
                {
                    var mixerAsset = AssetDatabase.LoadAssetAtPath<TimerMixer>(TimerMixer.kDefaultAssetPath);
                    if (mixerAsset == null)
                    {
                        EditorGUILayout.HelpBox(TimerMixerText.NotFoundLabel, MessageType.Error);
                        if (GUILayout.Button(TimerMixerText.Create))
                        {
                            var newAsset = ScriptableObject.CreateInstance<TimerMixer>();
                            AssetDatabase.CreateAsset(newAsset, TimerMixer.kDefaultAssetPath);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                    }
                    else
                    {
                        EditorGUILayout.ObjectField(TimerMixerText.CurrentAssetLabel, mixerAsset, typeof(TimerMixer), false);
                        if (s_CachedEditor == null)
                        {
                            Editor.CreateCachedEditor(mixerAsset, null, ref s_CachedEditor);
                            if (s_CachedEditor == null)
                            {
                                throw new System.InvalidOperationException("Failed to create editor for TimerMixer asset.");
                            }
                        }
                        s_CachedEditor.OnInspectorGUI();
                    }
                },
            };
        }
    }
}
