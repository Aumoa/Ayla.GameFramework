using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace Ayla;

public class SingletonManager : MonoBehaviour
{
    private static SingletonManager? s_Manager;
    private static Singleton[]? s_SingletonInstances;
    private static readonly List<Task> s_ContinuationQueue = new();

    public enum KnownEventNames
    {
        PostInitialize = 1,
        UserEvent = 100
    }

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        var gameObject = new GameObject("Singleton Manager", typeof(SingletonManager));
        DontDestroyOnLoad(gameObject);
        s_Manager = gameObject.GetComponent<SingletonManager>();

        using var scope1 = ListPool<Type>.Get(out var singletonTypes);
        ReflectionUtility.GetTypes(t => t.IsAssignableTo(typeof(Singleton)) && !t.IsAbstract, singletonTypes);
        using var scope2 = ListPool<Singleton>.Get(out var singletons);
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
                singletons.Add((Singleton)constructor.Invoke(Array.Empty<object>()));
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Failed to initialize singleton type {0}: {1}", type.FullName, e);
                continue;
            }
        }

        foreach (var singleton in singletons)
        {
            singleton.Initialize();
        }

        s_SingletonInstances = singletons.ToArray();
        Debug.LogFormat("Initialized {0} singleton(s).", s_SingletonInstances.Length);
    }

    private void OnGUI()
    {
        Debug.Assert(s_SingletonInstances != null, "Singleton instances have not been initialized.");
        foreach (var singleton in s_SingletonInstances!)
        {
            singleton.OnGUI();
        }
    }

    public static Task DispatchEventAsync(int eventId, CancellationToken cancellationToken = default)
    {
        lock (s_ContinuationQueue)
        {
            if (s_ContinuationQueue.Count > 0)
            {
                var task = ContinuationChain();
                s_ContinuationQueue.Add(task);
                return task;

                async Task ContinuationChain()
                {
                    await s_ContinuationQueue[^1];
                    await InternalDispatchEventAsync(eventId, cancellationToken);
                }
            }
            else
            {
                var task = InternalDispatchEventAsync(eventId, cancellationToken);
                s_ContinuationQueue.Add(task);
                return task;
            }
        }
    }

    private static async Task InternalDispatchEventAsync(int eventId, CancellationToken cancellationToken = default)
    {
        Debug.Assert(s_SingletonInstances != null, "Singleton instances have not been initialized.");
        var tasks = new List<Task>();
        foreach (var singleton in s_SingletonInstances!)
        {
            tasks.Add(singleton.OnEvent(eventId, cancellationToken).AsTask());
        }

        await Task.WhenAll(tasks);
        await Task.Yield();

        lock (s_ContinuationQueue)
        {
            s_ContinuationQueue.RemoveAt(0);
        }
    }
}
