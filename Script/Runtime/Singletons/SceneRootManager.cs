using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ayla;

public class SceneRootManager : Singleton<SceneRootManager, SceneRootManagerData>
{
    private readonly List<SceneRoot> m_SceneRoots = new();

    public void RegisterSceneRoot(SceneRoot sceneRoot)
    {
        m_SceneRoots.Add(sceneRoot);
    }

    public void UnregisterSceneRoot(SceneRoot sceneRoot)
    {
        m_SceneRoots.Remove(sceneRoot);
    }

    public SceneRoot? GetCurrentSceneRoot()
    {
#if UNITY_EDITOR
        foreach (var sceneRoot in m_SceneRoots)
        {
            Debug.Assert(sceneRoot);
        }
#endif

        return m_SceneRoots.LastOrDefault();
    }
}
