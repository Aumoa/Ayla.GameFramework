#nullable enable

using UnityEditor;
using UnityEngine;

namespace Ayla
{
    internal static class AbilityFrameworkRegister
    {
        private static Editor? s_CachedEditor;

        [SettingsProvider]
        public static SettingsProvider CreateAbilityFrameworkProvider()
        {
            return new SettingsProvider("Project/AbilityFramework", SettingsScope.Project)
            {
                label = "Ability Framework",
                guiHandler = _ =>
                {
                    var mixerAsset = AssetDatabase.LoadAssetAtPath<AbilityFramework>(AbilityFramework.kDefaultAssetPath);
                    if (mixerAsset == null)
                    {
                        EditorGUILayout.HelpBox(AbilityFrameworkText.NotFoundLabel, MessageType.Error);
                        if (GUILayout.Button(AbilityFrameworkText.Create))
                        {
                            var newAsset = ScriptableObject.CreateInstance<AbilityFramework>();
                            AssetDatabase.CreateAsset(newAsset, AbilityFramework.kDefaultAssetPath);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                    }
                    else
                    {
                        EditorGUILayout.ObjectField(AbilityFrameworkText.CurrentAssetLabel, mixerAsset, typeof(AbilityFramework), false);
                        if (s_CachedEditor == null)
                        {
                            Editor.CreateCachedEditor(mixerAsset, null, ref s_CachedEditor);
                            if (s_CachedEditor == null)
                            {
                                throw new System.InvalidOperationException("Failed to create editor for AbilityFramework asset.");
                            }
                        }
                        s_CachedEditor.OnInspectorGUI();
                    }
                }
            };
        }
    }
}
