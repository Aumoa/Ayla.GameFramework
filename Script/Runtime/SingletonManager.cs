using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace Ayla;

public class SingletonManager : MonoBehaviour
{
    [SerializeField]
    private Singleton[] m_Singletons = Array.Empty<Singleton>();

    private static SingletonManager? s_Instance;

    private static SingletonManager Instance
    {
        get
        {
            if (s_Instance != null)
            {
                return s_Instance;
            }

            s_Instance = FindAnyObjectByType<SingletonManager>();
            if (s_Instance == null)
            {
                throw new InvalidOperationException("SingletonManager has not been initialized yet.");
            }

            return s_Instance;
        }
    }

    [RuntimeInitializeOnLoadMethod]
    private static async Task Initialize()
    {
        var gameObject = new GameObject("Singleton Manager", typeof(SingletonManager));
        DontDestroyOnLoad(gameObject);
        var manager = gameObject.GetComponent<SingletonManager>();

        using var scope1 = ListPool<Type>.Get(out var singletonTypes);
        ReflectionUtility.GetTypes(t => t.IsAssignableTo(typeof(Singleton)) && !t.IsAbstract, singletonTypes);
        using var scope2 = ListPool<Singleton>.Get(out var singletons);

        Singleton.ConstructorContext.Begin(manager);
        using (new TimerScope("Construct singleton instances took {0}"))
        {
            try
            {
                foreach (var type in singletonTypes)
                {
                    var constructor = type.GetConstructor(Array.Empty<Type>());
                    if (constructor == null)
                    {
                        Debug.LogErrorFormat("Singleton type {0} does not have a parameterless constructor.", type.FullName);
                        continue;
                    }

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
            finally
            {
                Singleton.ConstructorContext.End();
            }
        }

        using var scope3 = ListPool<ValueTask>.Get(out var tasks);
        using (new TimerScope("Initialize singleton instances took {0} with async operations"))
        {
            foreach (var singleton in singletons)
            {
                tasks.Add(singleton.InitializeAsync(ApplicationMisc.ApplicationCancellationToken));
            }

            foreach (var task in tasks)
            {
                await task;
            }
        }

        tasks.Clear();

        using (new TimerScope("Post-initialize singleton instances took {0} with async operations"))
        {
            foreach (var singleton in singletons)
            {
                tasks.Add(singleton.PostInitializeAsync(ApplicationMisc.ApplicationCancellationToken));
            }

            foreach (var task in tasks)
            {
                await task;
            }
        }

        manager.m_Singletons = singletons.ToArray();
        Debug.LogFormat("Initialized {0} singleton(s).", singletons.Count);
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
