using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace Ayla;

public sealed class SceneUnloadingContext : IDisposable
{
    public abstract class UnloadTask : IDisposable
    {
        ~UnloadTask()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public virtual void OnReady()
        {
        }

        public abstract double Progress { get; }

        public abstract Task Task { get; }
    }

    public class UnloadAsyncOperationTask : UnloadTask
    {
        private readonly AsyncOperation m_AsyncOperation;
        private readonly TaskCompletionSource<object?> m_TCS = new();
        private readonly CancellationTokenRegistration? m_CancellationTokenRegistration;

        public UnloadAsyncOperationTask(AsyncOperation asyncOperation, CancellationToken cancellationToken = default)
        {
            m_AsyncOperation = asyncOperation;
            asyncOperation.completed += m_TCS.SetResult;
            if (cancellationToken.CanBeCanceled)
            {
                m_CancellationTokenRegistration = cancellationToken.Register(() => m_TCS.TrySetCanceled());
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_CancellationTokenRegistration.HasValue)
                {
                    m_CancellationTokenRegistration.Value.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        public override double Progress => m_AsyncOperation.isDone ? 1 : m_AsyncOperation.progress;

        public override Task Task => m_TCS.Task;
    }

    private readonly Scene m_NewEmptyScene;
    private readonly CancellationToken m_CancellationToken;
    private readonly List<UnloadTask> m_Tasks = new();

    internal SceneUnloadingContext(Scene newEmptyScene, CancellationToken cancellationToken)
    {
        m_NewEmptyScene = newEmptyScene;
        m_CancellationToken = cancellationToken;
    }

    ~SceneUnloadingContext()
    {
        Debug.LogErrorFormat("SceneUnloadingContext was not disposed properly. This may lead to memory leaks. Please ensure that Dispose() is called on the context when it is no longer needed.");
    }

    public void Dispose()
    {
        foreach (var task in m_Tasks)
        {
            task.Dispose();
        }

        m_Tasks.Clear();
        GC.SuppressFinalize(this);
    }

    public void AddTask(UnloadTask task)
    {
        lock (m_Tasks)
        {
            m_Tasks.Add(task);
        }
    }

    public void AddUnloadAllScenes()
    {
        var sceneCount = SceneManager.sceneCount;
        using (ListPool<Scene>.Get(out var scenesToUnload))
        {
            for (int i = 0; i < sceneCount; ++i)
            {
                var sceneToUnload = SceneManager.GetSceneAt(i);
                if (sceneToUnload != m_NewEmptyScene)
                {
                    scenesToUnload.Add(sceneToUnload);
                }
            }

            foreach (var sceneToUnload in scenesToUnload)
            {
                var asyncOperation = SceneManager.UnloadSceneAsync(sceneToUnload);
                AddTask(new UnloadAsyncOperationTask(asyncOperation, m_CancellationToken));
            }
        }
    }

    internal Task WhenAll() => Task.WhenAll(m_Tasks.Select(t => t.Task));
}
