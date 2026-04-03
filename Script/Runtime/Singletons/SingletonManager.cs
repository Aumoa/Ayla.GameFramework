using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
#if !UNITY_EDITOR
using UnityEngine.AddressableAssets;
#endif

namespace Ayla;

public class SingletonManager : MonoBehaviour
{
    [SerializeField]
    private Singleton[] m_Singletons = Array.Empty<Singleton>();

    private static SingletonManager? s_Instance;
    private static Task s_InstanceTask = null!;

    private static SingletonManager Instance
    {
        get
        {
            if (s_Instance != null)
            {
                return s_Instance;
            }

            s_Instance = FindAnyObjectByType<SingletonManager>();
            if (s_Instance != null)
            {
                return s_Instance;
            }

            s_InstanceTask.Wait();
            if (s_Instance == null)
            {
                throw new InvalidOperationException("SingletonManager instance is not initialized.");
            }

            return s_Instance;
        }
    }

    public static async ValueTask WaitForInitializeAsync()
    {
        await s_InstanceTask;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void Initialize()
    {
        s_InstanceTask = InitializeAsync();
    }

    private static async Task InitializeAsync()
    {
        var gameObject = new GameObject("Singleton Manager", typeof(SingletonManager));
        DontDestroyOnLoad(gameObject);
        var manager = gameObject.GetComponent<SingletonManager>();

        using var scope1 = ListPool<Type>.Get(out var singletonTypes);
        ReflectionUtility.GetTypes(t => t.IsAssignableTo(typeof(Singleton)) && !t.IsAbstract, singletonTypes);
        using var scope2 = ListPool<Singleton>.Get(out var singletons);

        using (new TimeLogScope("Load SingletonDefault asset took {0}"))
        {
            var singletonDefault = await LoadSingletonDefaultAssetAsync();
            Singleton.ConstructorContext.Begin(new Singleton.ConstructorArguments(manager, singletonDefault));
        }

        try
        {
            using (new TimeLogScope("Construct singleton instances took {0}"))
            {
                foreach (var type in singletonTypes)
                {
                    try
                    {
                        singletons.Add((Singleton)gameObject.AddComponent(type));
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("Failed to initialize singleton type {0}: {1}", type.FullName, e);
                        continue;
                    }
                }
            }
        }
        finally
        {
            Singleton.ConstructorContext.End();
        }

        using var scope3 = ListPool<ValueTask>.Get(out var tasks);
        using (new TimeLogScope("Initialize singleton instances took {0} with async operations"))
        {
            foreach (var singleton in singletons)
            {
                tasks.Add(singleton.InitializeAsync(ApplicationMisc.ApplicationCancellationToken));
            }

            await TaskUtility.WhenAll(tasks);
        }

        tasks.Clear();

        using (new TimeLogScope("Post-initialize singleton instances took {0} with async operations"))
        {
            foreach (var singleton in singletons)
            {
                tasks.Add(singleton.PostInitializeAsync(ApplicationMisc.ApplicationCancellationToken));
            }

            await TaskUtility.WhenAll(tasks);
        }

        tasks.Clear();

        using (new TimeLogScope("Post-initialize singleton instances took {0} with async operations"))
        {
            foreach (var singleton in singletons)
            {
                tasks.Add(singleton.StartAsync(ApplicationMisc.ApplicationCancellationToken));
            }

            await TaskUtility.WhenAll(tasks);
        }

        manager.m_Singletons = singletons.ToArray();
        Debug.LogFormat("Initialized {0} singleton(s).", singletons.Count);
    }

    private static
#if !UNITY_EDITOR
        async
#endif
        ValueTask<SingletonDefault> LoadSingletonDefaultAssetAsync()
    {
#if UNITY_EDITOR
        return new ValueTask<SingletonDefault>(AssetDatabase.LoadAssetAtPath<SingletonDefault>(SingletonDefault.kDefaultAssetPath));
#else
        return await Addressables.LoadAssetAsync<SingletonDefault>("Assets/Game/Settings/SingletonDefault.asset").Task;
#endif
    }

    /// <summary>
    /// Dispatches an event to all registered singletons asynchronously.
    /// </summary>
    /// <param name="eventId">The identifier of the event to dispatch.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask DispatchEventAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var instance = Instance;
        using var scope1 = ListPool<ValueTask>.Get(out var tasks);
        foreach (var singleton in instance.m_Singletons)
        {
            tasks.Add(singleton.OnEvent(eventId, cancellationToken));
        }

        foreach (var task in tasks)
        {
            await task;
        }
    }
}
