#nullable enable

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ayla
{
    public static class SingletonDefaultRegister
    {
        private static Editor? s_CachedEditor;

        [SettingsProvider]
        public static SettingsProvider CreateSingletonDefaultProvider()
        {
            return new SettingsProvider("Project/SingletonDefault", SettingsScope.Project)
            {
                label = "Singleton Default",
                guiHandler = _ =>
                {
                    var defaultAsset = AssetDatabase.LoadAssetAtPath<SingletonDefault>(SingletonDefault.kDefaultAssetPath);
                    if (defaultAsset == null)
                    {
                        EditorGUILayout.HelpBox(SingletonDefaultText.NotFoundLabel, MessageType.Error);
                        if (GUILayout.Button(SingletonDefaultText.Create))
                        {
                            var newAsset = ScriptableObject.CreateInstance<SingletonDefault>();
                            AssetDatabase.CreateAsset(newAsset, SingletonDefault.kDefaultAssetPath);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                    }
                    else
                    {
                        EditorGUILayout.ObjectField(SingletonDefaultText.CurrentAssetLabel, defaultAsset, typeof(SingletonDefault), false);

                        if (s_CachedEditor == null)
                        {
                            Editor.CreateCachedEditor(defaultAsset, null, ref s_CachedEditor);
                            if (s_CachedEditor == null)
                            {
                                throw new InvalidOperationException("Failed to create editor for SingletonDefault asset.");
                            }
                        }

                        s_CachedEditor.OnInspectorGUI();
                    }
                },
                deactivateHandler = () =>
                {
                    if (s_CachedEditor != null)
                    {
                        Object.DestroyImmediate(s_CachedEditor);
                        s_CachedEditor = null;
                    }
                }
            };
        }
    }
}
