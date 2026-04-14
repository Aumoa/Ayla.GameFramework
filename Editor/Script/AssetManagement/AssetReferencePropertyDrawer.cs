﻿#nullable enable

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ayla
{
    [CustomPropertyDrawer(typeof(AssetReference<>), true)]
    public class AssetReferencePropertyDrawer : PropertyDrawer
    {
        private SerializedProperty? m_CachedProperty;
        private SerializedProperty? m_AssetProperty;

        private Type? m_CachedAssetType;
        private Object? m_CachedEditorAsset;
        private GUIContent? m_TempContent;

#if WITH_ADDRESSABLES
        private SerializedProperty? m_AssetGUIDProperty;
        private string? m_CachedGUID;
#endif

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            TryCacheProperty(property);

            if (m_CachedAssetType == null)
            {
                EditorGUI.LabelField(position, label.text, "Invalid AssetReference");
                return;
            }

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
                    var newAsset = EditorGUI.ObjectField(position, displayLabel, currentAsset, m_CachedAssetType, false);
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

        private Object? GetCurrentEditorAsset()
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
                            ? AssetDatabase.LoadAssetAtPath(assetPath, m_CachedAssetType!)
                            : null;
                    }

                    return m_CachedEditorAsset;
                }
            }
#endif

            var orv = m_AssetProperty?.objectReferenceValue;
            if (orv != null)
            {
                return orv;
            }

            return null;
        }

        private void ApplyAssetChange(Object? newAsset)
        {
            if (newAsset == null)
            {
                if (m_AssetProperty != null)
                {
                    m_AssetProperty.objectReferenceValue = null;
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

#if WITH_ADDRESSABLES
            var assetPath = AssetDatabase.GetAssetPath(newAsset);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);

            if (AssetReferenceHelper.IsAddressablesAsset(guid))
            {
                if (m_AssetProperty != null)
                {
                    m_AssetProperty.objectReferenceValue = null;
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

            if (m_AssetProperty != null)
            {
                m_AssetProperty.objectReferenceValue = newAsset;
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
                AssetReferenceType.Reference => "(Linked)",
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

            m_CachedProperty = property;
            m_CachedEditorAsset = null;
#if WITH_ADDRESSABLES
            m_CachedGUID = null;
#endif

            if (property == null)
            {
                m_CachedAssetType = null;
                m_AssetProperty = null;
#if WITH_ADDRESSABLES
                m_AssetGUIDProperty = null;
#endif
            }
            else
            {
                var boxedValue = property.boxedValue;
                var referenceType = boxedValue.GetType();
                var implementationType = referenceType.FindImplementation(typeof(AssetReference<>));
                m_CachedAssetType = implementationType?.GetGenericArguments()[0];
                m_AssetProperty = property.FindPropertyRelative("m_Asset");
#if WITH_ADDRESSABLES
                m_AssetGUIDProperty = property.FindPropertyRelative("m_AssetGUID");
#endif
            }
        }
    }
}
