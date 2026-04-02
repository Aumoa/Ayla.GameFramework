using UnityEngine;

namespace Ayla;

public class SceneRoot : MonoBehaviour
{
    private void OnEnable()
    {
        SceneRootManager.Instance.RegisterSceneRoot(this);
    }

    private void OnDisable()
    {
        SceneRootManager.Instance.UnregisterSceneRoot(this);
    }
}
