#nullable enable

#if WITH_ADDRESSABLES

using System;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEditor.AddressableAssets;
using Object = UnityEngine.Object;

namespace Ayla
{
    public static partial class AssetReferenceHelper
    {
        public static bool TryGetAddressablesAsset<T>(string? guid, [NotNullWhen(true)] out T? asset) where T : Object
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                asset = null;
                return false;
            }

            if (!IsAddressablesAsset(guid))
            {
                asset = null;
                return false;
            }

            if (!GUID.TryParse(guid, out var g))
            {
                throw new InvalidOperationException($"Invalid GUID: {guid}");
            }

            asset = AssetDatabase.LoadAssetByGUID<T>(g);
            return true;
        }

        public static bool IsAddressablesAsset(string? guid)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return false;
            }

            var entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                return false;
            }

            return true;
        }
    }
}

#endif
