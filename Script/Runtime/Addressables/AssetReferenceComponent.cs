using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Ayla;

[Serializable]
public class AssetReferenceComponent<T> : AssetReferenceGameObject
    where T : Component
{
    public AssetReferenceComponent(string guid) : base(guid)
    {
    }

    public override bool ValidateAsset(Object obj)
    {
        if (obj is not GameObject gameObject)
        {
            return false;
        }

        return gameObject.TryGetComponent<T>(out _);
    }

#if UNITY_EDITOR
    public override bool ValidateAsset(string mainAssetPath)
    {
        var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(mainAssetPath);
        if (gameObject == null)
        {
            return false;
        }

        return gameObject.TryGetComponent<T>(out _);
    }
#endif
}
