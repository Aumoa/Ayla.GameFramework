#nullable enable

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Ayla
{
    [CustomPropertyDrawer(typeof(AssetReferenceComponent<>))]
    internal class AssetReferenceComponentPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty? m_CachedProperty;
        private Type? m_CachedComponentType;
        private string? m_AssetGUID;
        private GameObject? m_EditorAsset;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            TryCacheProperty(property);

            if (m_CachedComponentType == null)
            {
                EditorGUI.LabelField(position, label.text, "Invalid AssetReferenceComponent");
                return;
            }

            var assetGUIDProperty = property.FindPropertyRelative("m_AssetGUID");
            var assetGUID = assetGUIDProperty.stringValue;
            if (m_AssetGUID != assetGUID)
            {
                m_AssetGUID = assetGUID;
                if (GUID.TryParse(assetGUID, out var g))
                {
                    m_EditorAsset = AssetDatabase.LoadAssetByGUID<GameObject>(g);
                }
                else
                {
                    m_EditorAsset = null;
                }
            }

            Component? currentComponent = m_EditorAsset ? m_EditorAsset.GetComponent(m_CachedComponentType) : null;
            var newComponent = (Component?)EditorGUI.ObjectField(position, label, currentComponent, m_CachedComponentType, false);
            if (GUI.changed)
            {
                var newGameObject = newComponent ? newComponent.gameObject : null;
                if (newGameObject != m_EditorAsset)
                {
                    var argo = (AssetReferenceGameObject)property.boxedValue;
                    argo.SetEditorAsset(newGameObject!);
                    assetGUIDProperty.stringValue = argo.AssetGUID;
                    m_EditorAsset = newGameObject;
                }
            }
        }

        private void TryCacheProperty(SerializedProperty property)
        {
            if (m_CachedProperty == property)
            {
                return;
            }

            m_CachedProperty = property;
            if (property == null)
            {
                m_CachedComponentType = null;
            }
            else
            {
                var boxedValue = property.boxedValue;
                var referenceType = boxedValue.GetType();
                m_CachedComponentType = referenceType.GetGenericArguments()[0];
            }
        }
    }
}
