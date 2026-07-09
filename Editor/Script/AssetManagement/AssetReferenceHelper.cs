#nullable enable

using System;
using UnityEditor;
using GUID = UnityEngine.GUID;
using Object = UnityEngine.Object;

namespace Ayla
{
    public static partial class AssetReferenceHelper
    {
        public static void SetEditorAsset<T>(SerializedProperty property, T? value) where T : Object
        {
            if (value == null)
            {
                SetEmpty();
                return;
            }

            var assetPath = AssetDatabase.GetAssetPath(value);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                SetEmpty();
                return;
            }

#if WITH_ADDRESSABLES
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (IsAddressablesAsset(guid))
            {
                property.FindPropertyRelative("m_Asset").objectReferenceValue = null;
                var ag = property.FindPropertyRelative("m_AssetGUID");
                ag.stringValue = guid;
                return;
            }
#endif

#if WITH_ADDRESSABLES
            property.FindPropertyRelative("m_Asset").objectReferenceValue = value;
            property.FindPropertyRelative("m_AssetGUID").stringValue = string.Empty;
#endif

            return;

            void SetEmpty()
            {
                property.FindPropertyRelative("m_Asset").objectReferenceValue = null;
#if WITH_ADDRESSABLES
                property.FindPropertyRelative("m_AssetGUID").stringValue = string.Empty;
#endif
            }
        }

        public static T? GetEditorAsset<T>(SerializedProperty property) where T : Object
        {
#if WITH_ADDRESSABLES
            var ag = property.FindPropertyRelative("m_AssetGUID");
            if (ag != null)
            {
                if (!GUID.TryParse(ag.stringValue, out var g))
                {
                    throw new InvalidOperationException("Invalid GUID: " + ag.stringValue);
                }

                return AssetDatabase.LoadAssetByGUID<T>(g);
            }
#endif

            return (T?)(object?)property.FindPropertyRelative("m_Asset").objectReferenceValue;
        }
    }
}
