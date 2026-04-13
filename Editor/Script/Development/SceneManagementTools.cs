#nullable enable

using UnityEditor;
using UnityEngine;

namespace Ayla
{
    [GameFrameworkCategory, DefaultOrder(0)]
    internal class SceneManagementTools : DevelopmentTools
    {
        private class Serializer : ScriptableObject
        {
            public AssetReference<SceneRoot> Ref;
        }

        private static SerializedProperty? s_Serializer;

        private GUILayoutOption? m_NotExpandWidth;

        private SingletonDefault? m_DefaultAsset;
        private SerializedObject? m_SceneRootSerializedCache;
        private SerializedProperty? m_SceneRootInitialSceneCache;
        private SerializedProperty? m_SceneRootEditorOverrideSceneCache;

        protected override void OnGUI(in DrawingArgs drawingArgs)
        {
            if (m_DefaultAsset == null)
            {
                m_DefaultAsset = AssetDatabase.LoadAssetAtPath<SingletonDefault>(SingletonDefault.kDefaultAssetPath);
            }

            m_NotExpandWidth ??= GUILayout.ExpandWidth(false);

            if (m_DefaultAsset == null)
            {
                using (GUIScope.Horizontal())
                {
                    GUILayout.Label(SingletonDefaultText.NotFoundLabel, m_NotExpandWidth);
                    if (GUILayout.Button(SingletonDefaultText.Create, m_NotExpandWidth))
                    {
                        var defaultAsset = ScriptableObject.CreateInstance<SingletonDefault>();
                        AssetDatabase.CreateAsset(defaultAsset, SingletonDefault.kDefaultAssetPath);
                        AssetDatabase.SaveAssets();
                    }
                }
                return;
            }

            EditorGUILayout.ObjectField(SingletonDefaultText.CurrentAssetLabel, m_DefaultAsset, typeof(SingletonDefault), false);

            var data = (SceneRootManagerData?)m_DefaultAsset.GetData(typeof(SceneRootManager));
            if (data == null)
            {
                m_SceneRootSerializedCache = null;
                m_SceneRootInitialSceneCache = null;
                m_SceneRootEditorOverrideSceneCache = null;
                GUILayout.Label(SingletonDefaultText.DataNotFoundLabel);
                return;
            }

            if (m_SceneRootSerializedCache == null || m_SceneRootInitialSceneCache == null || m_SceneRootSerializedCache.targetObject != data)
            {
                m_SceneRootSerializedCache = new SerializedObject(data);
                m_SceneRootInitialSceneCache = m_SceneRootSerializedCache.FindProperty("InitialScene");
                m_SceneRootEditorOverrideSceneCache = m_SceneRootSerializedCache.FindProperty("EditorOverrideScene");
            }

            m_SceneRootSerializedCache.Update();

            EditorGUILayout.PropertyField(m_SceneRootInitialSceneCache, EditorGUIUtility.TrTempContent(SingletonDefaultText.InitialSceneLabel));
            EditorGUILayout.PropertyField(m_SceneRootEditorOverrideSceneCache, EditorGUIUtility.TrTempContent(SingletonDefaultText.EditorOverrideSceneLabel));

            m_SceneRootSerializedCache.ApplyModifiedProperties();

            const string kHotReloadScene = nameof(SceneManagementTools) + "." + "m_HotReloadScene";
            var hotReloadSceneGUID = EditorPrefs.GetString(kHotReloadScene);
            var hotReloadScenePath = string.IsNullOrEmpty(hotReloadSceneGUID) ? string.Empty : AssetDatabase.GUIDToAssetPath(hotReloadSceneGUID);
            var hotReloadScene = string.IsNullOrEmpty(hotReloadScenePath) ? null : AssetDatabase.LoadAssetAtPath<SceneRoot>(hotReloadScenePath);
            using (GUIScope.Changed())
            using (EditorGUIScope.Horizontal())
            {
                if (s_Serializer == null || s_Serializer.serializedObject == null || s_Serializer.serializedObject.targetObject == null)
                {
                    var so = ScriptableObject.CreateInstance<Serializer>();
                    Object.DontDestroyOnLoad(so);
                    var so2 = new SerializedObject(so);
                    s_Serializer = so2.FindProperty("Ref");
                }

                s_Serializer.serializedObject.Update();
                EditorGUILayout.PropertyField(s_Serializer, EditorGUIUtility.TrTempContent(SceneManagementToolText.HotReloadSceneLabel));
                s_Serializer.serializedObject.ApplyModifiedProperties();

                if (GUI.changed)
                {
                    hotReloadScene = AssetReferenceHelper.GetEditorAsset<SceneRoot>(s_Serializer);
                    if (hotReloadScene)
                    {
                        hotReloadScenePath = AssetDatabase.GetAssetPath(hotReloadScene);
                        hotReloadSceneGUID = AssetDatabase.AssetPathToGUID(hotReloadScenePath);
                    }
                    else
                    {
                        hotReloadScenePath = string.Empty;
                        hotReloadSceneGUID = string.Empty;
                    }

                    EditorPrefs.SetString(kHotReloadScene, hotReloadSceneGUID);
                }

                using (GUIScope.Disabled(!Application.isPlaying || string.IsNullOrWhiteSpace(hotReloadSceneGUID)))
                {
                    if (GUILayout.Button(SceneManagementToolText.HotReloadButton) && Application.isPlaying && SceneRootManager.TryGetInstance(out var instance))
                    {

                        AssetReferenceHelper.SetEditorAsset(s_Serializer, hotReloadScene);
                        var sr = ((Serializer)s_Serializer.serializedObject.targetObject).Ref;
                        _ = SceneRootManager.Instance.LoadSceneAsync(sr);
                    }
                }
            }
        }
    }
}
