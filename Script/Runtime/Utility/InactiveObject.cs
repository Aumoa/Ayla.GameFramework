using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Ayla;

public static class InactiveObject
{
#if UNITY_EDITOR
    private class InactiveObjectRootSerializer : MonoBehaviour
    {
        public void LoadSerialized()
        {
            s_InactiveObjectRoot = gameObject;
            s_Initialized = true;
        }
    }
#endif

    private readonly struct InactiveObjectLoader : IDisposable
    {
        public readonly GameObject m_InstancedInactivePrefab;

        public InactiveObjectLoader(GameObject prefab)
        {
            m_InstancedInactivePrefab = Instantiate(prefab);
        }

        public void Dispose()
        {
            Object.Destroy(m_InstancedInactivePrefab);
        }

        public static GameObject Instantiate(GameObject prefab)
        {
            var instance = Object.Instantiate(prefab);
            Object.DontDestroyOnLoad(instance);
            instance.SetActive(false);
#if UNITY_EDITOR
            instance.hideFlags |= HideFlags.DontSave | HideFlags.HideInHierarchy;
#endif
            return instance;
        }
    }

    private static bool s_Initialized;
    private static GameObject? s_InactiveObjectRoot;

    private static void InitializeOnDemand()
    {
        if (s_Initialized)
        {
            return;
        }

#if UNITY_EDITOR
        var inactiveObjectRoot = Object.FindAnyObjectByType<InactiveObjectRootSerializer>(FindObjectsInactive.Include);
        if (inactiveObjectRoot != null)
        {
            inactiveObjectRoot.LoadSerialized();
            return;
        }
#endif

        s_InactiveObjectRoot = new GameObject("Inactive Object Root"
#if UNITY_EDITOR
            , typeof(InactiveObjectRootSerializer)
#endif
        );

#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            Object.DontDestroyOnLoad(s_InactiveObjectRoot);

#if UNITY_EDITOR
        s_InactiveObjectRoot.hideFlags |=
            HideFlags.DontSave |
            HideFlags.HideInHierarchy;
#endif
        s_InactiveObjectRoot.SetActive(false);
        s_Initialized = true;
    }

    public static T Instantiate<T>(T original) where T : Component
    {
        var gameObject = Instantiate(original.gameObject);
        return gameObject.GetComponent<T>();
    }

    public static T Instantiate<T>(T original, Transform parent) where T : Component
    {
        var gameObject = Instantiate(original.gameObject, parent);
        return gameObject.GetComponent<T>();
    }

    public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Component
    {
        var gameObject = Instantiate(original.gameObject, position, rotation);
        return gameObject.GetComponent<T>();
    }

    public static async ValueTask<T[]> InstantiateAsync<T>(T original, CancellationToken cancellationToken = default) where T : Component
    {
        var gameObjects = await InstantiateAsync(original.gameObject, cancellationToken);
        return gameObjects.Select(p => p.GetComponent<T>()).ToArray();
    }

    public static async ValueTask<T[]> InstantiateAsync<T>(T original, int count, CancellationToken cancellationToken = default) where T : Component
    {
        var gameObjects = await InstantiateAsync(original.gameObject, count, cancellationToken);
        return gameObjects.Select(p => p.GetComponent<T>()).ToArray();
    }

    public static async ValueTask<T[]> InstantiateAsync<T>(T original, Transform parent, CancellationToken cancellationToken = default) where T : Component
    {
        var gameObjects = await InstantiateAsync(original.gameObject, parent, cancellationToken);
        return gameObjects.Select(p => p.GetComponent<T>()).ToArray();
    }

    public static async ValueTask<T[]> InstantiateAsync<T>(T original, int count, Transform parent, CancellationToken cancellationToken = default) where T : Component
    {
        var gameObjects = await InstantiateAsync(original.gameObject, count, parent, cancellationToken);
        return gameObjects.Select(p => p.GetComponent<T>()).ToArray();
    }

    public static async ValueTask<T[]> InstantiateAsync<T>(T original, Vector3 position, Quaternion rotation, CancellationToken cancellationToken = default) where T : Component
    {
        var gameObjects = await InstantiateAsync(original.gameObject, position, rotation, cancellationToken);
        return gameObjects.Select(p => p.GetComponent<T>()).ToArray();
    }

    public static async ValueTask<T[]> InstantiateAsync<T>(T original, int count, Vector3 position, Quaternion rotation, CancellationToken cancellationToken = default) where T : Component
    {
        var gameObjects = await InstantiateAsync(original.gameObject, count, position, rotation, cancellationToken);
        return gameObjects.Select(p => p.GetComponent<T>()).ToArray();
    }

    public static async ValueTask<T[]> InstantiateAsync<T>(T original, Vector3[] positions, Quaternion[] rotations, CancellationToken cancellationToken = default) where T : Component
    {
        var gameObjects = await InstantiateAsync(original.gameObject, positions, rotations, cancellationToken);
        return gameObjects.Select(p => p.GetComponent<T>()).ToArray();
    }

    public static async ValueTask<T[]> InstantiateAsync<T>(T original, int count, Vector3[] positions, Quaternion[] rotations, CancellationToken cancellationToken = default) where T : Component
    {
        var gameObjects = await InstantiateAsync(original.gameObject, count, positions, rotations, cancellationToken);
        return gameObjects.Select(p => p.GetComponent<T>()).ToArray();
    }

    public static GameObject Instantiate(GameObject original)
    {
        InitializeOnDemand();
        var instancedGameObject = Object.Instantiate(original, s_InactiveObjectRoot!.transform);
        instancedGameObject.SetActive(false);
        SceneManager.MoveGameObjectToScene(instancedGameObject, SceneManager.GetActiveScene());
        return instancedGameObject;
    }

    public static GameObject Instantiate(GameObject original, Transform parent)
    {
        InitializeOnDemand();
        var instancedGameObject = Object.Instantiate(original, s_InactiveObjectRoot!.transform);
        instancedGameObject.SetActive(false);
        instancedGameObject.transform.SetParent(parent, false);
        return instancedGameObject;
    }

    public static GameObject Instantiate(GameObject original, Vector3 position, Quaternion rotation)
    {
        InitializeOnDemand();
        var instancedGameObject = Object.Instantiate(original, s_InactiveObjectRoot!.transform);
        instancedGameObject.SetActive(false);
        SceneManager.MoveGameObjectToScene(instancedGameObject, SceneManager.GetActiveScene());
        instancedGameObject.transform.SetLocalPositionAndRotation(position, rotation);
        return instancedGameObject;
    }

    public static ValueTask<GameObject[]> InstantiateAsync(GameObject original, CancellationToken cancellationToken = default)
    {
        return InstantiateAsync(original, 1, cancellationToken);
    }

    public static async ValueTask<GameObject[]> InstantiateAsync(GameObject original, int count, CancellationToken cancellationToken = default)
    {
        InitializeOnDemand();
        using var template = new InactiveObjectLoader(original);
        var op = Object.InstantiateAsync(template.m_InstancedInactivePrefab, count);
        await using var reg = cancellationToken.Register(() => op.Cancel());
        var instancedGameObjects = await op;
        return instancedGameObjects;
    }

    public static ValueTask<GameObject[]> InstantiateAsync(GameObject original, Transform parent, CancellationToken cancellationToken = default)
    {
        return InstantiateAsync(original, 1, parent, cancellationToken);
    }

    public static async ValueTask<GameObject[]> InstantiateAsync(GameObject original, int count, Transform parent, CancellationToken cancellationToken = default)
    {
        InitializeOnDemand();
        using var template = new InactiveObjectLoader(original);
        var op = Object.InstantiateAsync(template.m_InstancedInactivePrefab, count, parent);
        await using var reg = cancellationToken.Register(() => op.Cancel());
        var instancedGameObjects = await op;
        return instancedGameObjects;
    }

    public static ValueTask<GameObject[]> InstantiateAsync(GameObject original, Vector3 position, Quaternion rotation, CancellationToken cancellationToken = default)
    {
        return InstantiateAsync(original, 1, position, rotation, cancellationToken);
    }

    public static async ValueTask<GameObject[]> InstantiateAsync(GameObject original, int count, Vector3 position, Quaternion rotation, CancellationToken cancellationToken = default)
    {
        InitializeOnDemand();
        using var template = new InactiveObjectLoader(original);
        var op = Object.InstantiateAsync(template.m_InstancedInactivePrefab, count, position, rotation);
        await using var reg = cancellationToken.Register(() => op.Cancel());
        var instancedGameObjects = await op;
        return instancedGameObjects;
    }

    public static ValueTask<GameObject[]> InstantiateAsync(GameObject original, Vector3[] positions, Quaternion[] rotations, CancellationToken cancellationToken = default)
    {
        return InstantiateAsync(original, 1, positions, rotations, cancellationToken);
    }

    public static ValueTask<GameObject[]> InstantiateAsync(GameObject original, int count, ReadOnlySpan<Vector3> positions, ReadOnlySpan<Quaternion> rotations, CancellationToken cancellationToken = default)
    {
        if (count != positions.Length || count != rotations.Length)
        {
            throw new ArgumentException();
        }

        InitializeOnDemand();
        var template = new InactiveObjectLoader(original);
        var op = Object.InstantiateAsync(original, count, positions, rotations);
        var reg = cancellationToken.Register(() => op.Cancel());
        return InternalAsync();

        async ValueTask<GameObject[]> InternalAsync()
        {
            using var innerTemplate = template;
            using var innerReg = reg;
            var instancedGameObjects = await op;
            return instancedGameObjects;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitializeOnLoad()
    {
        s_Initialized = false;
    }
}