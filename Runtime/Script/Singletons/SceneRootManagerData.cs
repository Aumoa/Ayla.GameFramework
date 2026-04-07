using UnityEngine.AddressableAssets;

namespace Ayla
{
    public class SceneRootManagerData : SingletonData
    {
        public AssetReferenceGameObject InitialScene;

#if UNITY_EDITOR
        public AssetReferenceGameObject EditorOverrideScene;
#endif
    }
}
