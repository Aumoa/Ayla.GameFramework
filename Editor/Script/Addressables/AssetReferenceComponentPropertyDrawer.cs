﻿#nullable enable

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ayla
{
    [CustomPropertyDrawer(typeof(AssetReference<>), true)]
    internal class AssetReferencePropertyDrawer : PropertyDrawer
    {
        private SerializedProperty? m_CachedProperty;
        private SerializedProperty? m_AssetProperty;
        private SerializedProperty? m_AssetGUIDProperty;

        private Type? m_CachedAssetType;
        private string? m_CachedGUID;
        private Object? m_CachedEditorAsset;

        private static readonly Color s_LinkedColor = new(0.7f, 0.85f, 1f);
#if WITH_ADDRESSABLES
        private static readonly Color s_SoftReferenceColor = new(0.7f, 1f, 0.7f);
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
                : new GUIContent($"{label.text} {typeLabel}", label.image, label.tooltip);

            EditorGUI.BeginProperty(position, displayLabel, property);

            using (GUIScope.Color(GetAssetTypeColor(assetType)))
            using (GUIScope.Changed())
            {
                var newAsset = EditorGUI.ObjectField(position, displayLabel, currentAsset, m_CachedAssetType, false);
                if (GUI.changed)
                {
                    ApplyAssetChange(newAsset);
                }
            }

            EditorGUI.EndProperty();
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
            if (m_AssetProperty?.objectReferenceValue != null)
            {
                return m_AssetProperty.objectReferenceValue;
            }

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
                m_CachedGUID = null;
                return;
            }

#if WITH_ADDRESSABLES
            var assetPath = AssetDatabase.GetAssetPath(newAsset);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);

            if (IsAddressableAsset(guid))
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
            m_CachedGUID = null;
        }

#if WITH_ADDRESSABLES
        private static bool IsAddressableAsset(string guid)
        {
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            return settings != null && settings.FindAssetEntry(guid) != null;
        }
#endif

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
                AssetReferenceType.Reference => s_LinkedColor,
#if WITH_ADDRESSABLES
                AssetReferenceType.SoftReference => s_SoftReferenceColor,
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
            m_CachedGUID = null;

            if (property == null)
            {
                m_CachedAssetType = null;
                m_AssetProperty = null;
                m_AssetGUIDProperty = null;
            }
            else
            {
                var boxedValue = property.boxedValue;
                var referenceType = boxedValue.GetType();
                var implementationType = referenceType.FindImplementation(typeof(AssetReference<>));
                m_CachedAssetType = implementationType?.GetGenericArguments()[0];
                m_AssetProperty = property.FindPropertyRelative("m_Asset");
                m_AssetGUIDProperty = property.FindPropertyRelative("m_AssetGUID");
            }
        }
    }
}
