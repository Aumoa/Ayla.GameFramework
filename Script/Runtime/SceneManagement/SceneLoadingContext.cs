using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ayla;

public sealed class SceneLoadingContext : IDisposable
{
    public abstract class LoadTask : IDisposable
    {
        ~LoadTask()
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

        public abstract double Progress { get; }

        public abstract Task Task { get; }
    }

    private readonly CancellationToken m_CancellationToken;
    private readonly List<LoadTask> m_Tasks = new();

    internal SceneLoadingContext(CancellationToken cancellationToken)
    {
        m_CancellationToken = cancellationToken;
    }

    ~SceneLoadingContext()
    {
        Debug.LogErrorFormat("SceneLoadingContext was not disposed properly. This may lead to memory leaks. Please ensure that Dispose() is called on the context when it is no longer needed.");
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

    public void AddTask(LoadTask task)
    {
        lock (m_Tasks)
        {
            m_Tasks.Add(task);
        }
    }

    internal Task WhenAll() => Task.WhenAll(m_Tasks.Select(t => t.Task));
}
