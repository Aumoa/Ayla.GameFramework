using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Ayla;

[Serializable]
public class AssetReferenceComponent<T> : AssetReferenceGameObject
    where T : Component
{
    public AssetReferenceComponent(string guid) : base(guid)
    {
    }
}
