#nullable enable

using System;
using UnityEditor;
using UnityEngine.AddressableAssets;

namespace Ayla
{
    [Serializable]
    public class SceneReference :
#if UNITY_EDITOR
        AssetReferenceT<SceneAsset>
#else
        AssetReference
#endif
    {
#if UNITY_EDITOR
        public SceneReference(string guid) : base(guid)
        {
        }
#endif
    }
}
