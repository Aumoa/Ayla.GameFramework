#nullable enable

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ayla
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferencePropertyDrawer : PropertyDrawer
    {
        private SerializedProperty? m_CachedProperty;
        private SerializedProperty? m_AssetProperty;
        private SerializedProperty? m_BuildIndexProperty;

        private SceneAsset? m_CachedEditorAsset;
        private GUIContent? m_TempContent;

#if WITH_ADDRESSABLES
        private SerializedProperty? m_AssetGUIDProperty;
        private string? m_CachedGUID;
#endif

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            TryCacheProperty(property);
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            TryCacheProperty(property);

            if (property.isExpanded)
            {
                property.isExpanded = false;
            }

            var assetType = GetCurrentAssetType();
            var currentAsset = GetCurrentEditorAsset();

            var typeLabel = GetAssetTypeLabel(assetType);
            var displayLabel = string.IsNullOrEmpty(typeLabel)
                ? label
                : TempContent($"{label.text} {typeLabel}", label.image, label.tooltip);

            EditorGUI.BeginProperty(position, displayLabel, property);

            try
            {
                using (GUIScope.Color(GetAssetTypeColor(assetType)))
                using (GUIScope.Changed())
                {
                    var newAsset = (SceneAsset?)EditorGUI.ObjectField(position, displayLabel, currentAsset, typeof(SceneAsset), false);
                    if (GUI.changed)
                    {
                        ApplyAssetChange(newAsset);
                    }
                }
            }
            finally
            {
                EditorGUI.EndProperty();
            }

            return;

            GUIContent TempContent(string text, Texture? image, string tooltip)
            {
                m_TempContent ??= new GUIContent();
                m_TempContent.text = text;
                m_TempContent.image = image;
                m_TempContent.tooltip = tooltip;
                return m_TempContent;
            }
        }

        private AssetReferenceType GetCurrentAssetType()
        {
#if WITH_ADDRESSABLES
            if (m_AssetGUIDProperty != null && !string.IsNullOrEmpty(m_AssetGUIDProperty.stringValue))
            {
                return AssetReferenceType.SoftReference;
            }
#endif

            if (m_AssetProperty != null && m_AssetProperty.objectReferenceValue != null)
            {
                return AssetReferenceType.Reference;
            }

            return AssetReferenceType.None;
        }

        private SceneAsset? GetCurrentEditorAsset()
        {
#if WITH_ADDRESSABLES
            if (m_AssetGUIDProperty != null)
            {
                var guidStr = m_AssetGUIDProperty.stringValue;
                if (!string.IsNullOrEmpty(guidStr))
                {
                    if (m_CachedGUID != guidStr || m_CachedEditorAsset == null)
                    {
                        m_CachedGUID = guidStr;
                        var assetPath = AssetDatabase.GUIDToAssetPath(guidStr);
                        m_CachedEditorAsset = !string.IsNullOrEmpty(assetPath)
                            ? AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath)
                            : null;
                    }

                    return m_CachedEditorAsset;
                }
            }
#endif

            var orv = m_AssetProperty?.objectReferenceValue;
            if (orv != null)
            {
                return (SceneAsset)orv;
            }

            return null;
        }

        private void ApplyAssetChange(SceneAsset? newAsset)
        {
            if (newAsset == null)
            {
                if (m_AssetProperty != null)
                {
                    m_AssetProperty.objectReferenceValue = null;
                }
                if (m_BuildIndexProperty != null)
                {
                    m_BuildIndexProperty.intValue = -1;
                }
#if WITH_ADDRESSABLES
                if (m_AssetGUIDProperty != null)
                {
                    m_AssetGUIDProperty.stringValue = string.Empty;
                }
#endif

                m_CachedEditorAsset = null;
#if WITH_ADDRESSABLES
                m_CachedGUID = null;
#endif
                return;
            }

            var assetPath = AssetDatabase.GetAssetPath(newAsset);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);

#if WITH_ADDRESSABLES

            if (AssetReferenceHelper.IsAddressablesAsset(guid))
            {
                if (m_AssetProperty != null)
                {
                    m_AssetProperty.objectReferenceValue = null;
                }
                if (m_BuildIndexProperty != null)
                {
                    m_BuildIndexProperty.intValue = -1;
                }
                if (m_AssetGUIDProperty != null)
                {
                    m_AssetGUIDProperty.stringValue = guid;
                }
                m_CachedEditorAsset = newAsset;
                m_CachedGUID = guid;
                return;
            }
#endif

            GUID.TryParse(guid, out var g);
            var scenes = EditorBuildSettings.scenes;
            int buildIndex = -1;
            for (int i = 0; i < scenes.Length; ++i)
            {
                if (scenes[i].guid == g)
                {
                    buildIndex = i;
                    break;
                }
            }

            if (buildIndex == -1)
            {
                if (EditorUtility.DisplayDialog(SceneReferenceText.AddToBuildSceneTitle, SceneReferenceText.AddToBuildSceneMessage, SceneReferenceText.Confirm, SceneReferenceText.Cancel))
                {
                    var buildScene = new EditorBuildSettingsScene(g, true);
                    scenes = EditorBuildSettings.scenes.Append(buildScene).Distinct().ToArray();
                    EditorBuildSettings.scenes = scenes;
                }
                else
                {
                    Debug.LogErrorFormat("Scene '{0}' is not included in the build settings. Please add it to the build settings to use it as a reference.", newAsset.name);
                    newAsset = null;
                }
            }

            if (m_AssetProperty != null)
            {
                m_AssetProperty.objectReferenceValue = newAsset;
            }
            if (m_BuildIndexProperty != null)
            {
                m_BuildIndexProperty.intValue = buildIndex;
            }
#if WITH_ADDRESSABLES
            if (m_AssetGUIDProperty != null)
            {
                m_AssetGUIDProperty.stringValue = string.Empty;
            }
#endif
            m_CachedEditorAsset = newAsset;
#if WITH_ADDRESSABLES
            m_CachedGUID = null;
#endif
        }

        private static string GetAssetTypeLabel(AssetReferenceType assetType)
        {
            return assetType switch
            {
                AssetReferenceType.Reference => "(Built)",
#if WITH_ADDRESSABLES
                AssetReferenceType.SoftReference => "(Soft)",
#endif
                _ => string.Empty
            };
        }

        private static Color GetAssetTypeColor(AssetReferenceType assetType)
        {
            return assetType switch
            {
                AssetReferenceType.Reference => Stylesheet.LinkedReferenceColor,
#if WITH_ADDRESSABLES
                AssetReferenceType.SoftReference => Stylesheet.SoftReferenceColor,
#endif
                _ => Color.white
            };
        }

        private void TryCacheProperty(SerializedProperty property)
        {
            if (m_CachedProperty == property)
            {
                return;
            }

            m_CachedProperty = null;
            m_CachedEditorAsset = null;
#if WITH_ADDRESSABLES
            m_CachedGUID = null;
#endif

            if (property == null)
            {
                m_AssetProperty = null;
                m_BuildIndexProperty = null;
#if WITH_ADDRESSABLES
                m_AssetGUIDProperty = null;
#endif
            }
            else
            {
                m_AssetProperty = property.FindPropertyRelative("m_Asset");
                m_BuildIndexProperty = m_AssetProperty.Copy();
                m_BuildIndexProperty.Next(false);
#if WITH_ADDRESSABLES
                m_AssetGUIDProperty = property.FindPropertyRelative("m_AssetGUID");
#endif
            }
        }
    }
}
