namespace Ayla
{
    public class SceneRootManagerData : SingletonData
    {
        public AssetReferenceComponent<SceneRoot> InitialScene;

#if UNITY_EDITOR
        public AssetReferenceComponent<SceneRoot> EditorOverrideScene;
#endif
    }
}
