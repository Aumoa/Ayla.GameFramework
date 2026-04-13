namespace Ayla
{
    public class SceneRootManagerData : SingletonData
    {
        public AssetReference<SceneRoot> InitialScene;

#if UNITY_EDITOR
        public AssetReference<SceneRoot> EditorOverrideScene;
#endif
    }
}
