using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Ayla;

[GameFrameworkCategory, DefaultOrder(0)]
internal class SceneManagementTools : DevelopmentTools
{
    private GUILayoutOption? m_NotExpandWidth;

    private SingletonDefault? m_DefaultAsset;

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
            GUILayout.Label(SingletonDefaultText.DataNotFoundLabel);
            return;
        }

        using (GUIScope.Changed())
        {
            var initialScene = data.InitialScene.editorAsset;
            var newInitialScene = (SceneRoot?)EditorGUILayout.ObjectField(SingletonDefaultText.InitialSceneLabel, initialScene ? initialScene.GetComponent<SceneRoot>() : null, typeof(SceneRoot), false);
            if (GUI.changed)
            {
                Undo.RecordObject(data, "Change initial scene");
                data.InitialScene.SetEditorAsset(newInitialScene ? newInitialScene.gameObject : null!);
                EditorUtility.SetDirty(data);
            }
        }

        using (GUIScope.Changed())
        {
            var editorOverrideScene = data.EditorOverrideScene.editorAsset;
            var newEditorOverrideScene = (SceneRoot?)EditorGUILayout.ObjectField(SingletonDefaultText.EditorOverrideSceneLabel, editorOverrideScene ? editorOverrideScene.GetComponent<SceneRoot>() : null, typeof(SceneRoot), false);
            if (GUI.changed)
            {
                Undo.RecordObject(data, "Change editor override scene");
                data.EditorOverrideScene.SetEditorAsset(newEditorOverrideScene ? newEditorOverrideScene.gameObject : null!);
                EditorUtility.SetDirty(data);
            }
        }

        const string kHotReloadScene = nameof(SceneManagementTools) + "." + "m_HotReloadScene";
        var hotReloadSceneGUID = EditorPrefs.GetString(kHotReloadScene);
        var hotReloadScenePath = string.IsNullOrEmpty(hotReloadSceneGUID) ? string.Empty : AssetDatabase.GUIDToAssetPath(hotReloadSceneGUID);
        var hotReloadScene = string.IsNullOrEmpty(hotReloadScenePath) ? null : AssetDatabase.LoadAssetAtPath<SceneRoot>(hotReloadScenePath);
        using (GUIScope.Changed())
        using (EditorGUIScope.Horizontal())
        {
            hotReloadScene = (SceneRoot?)EditorGUILayout.ObjectField(SceneManagementToolText.HotReloadSceneLabel, hotReloadScene, typeof(SceneRoot), false);
            if (GUI.changed)
            {
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
                    instance.LoadSceneAsync(new AssetReferenceGameObject(hotReloadSceneGUID))
                        .Forget();
                }
            }
        }
    }
}
