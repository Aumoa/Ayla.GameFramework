using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

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

    public class AdditiveSceneTask : LoadTask, AddressablesTaskExtensions.IProgressCallback
    {
        private const double kProgressWeightForInstantiate = 0.1;
        private const double kProgressWeightForActivation = 0.1;

        private double m_Progress;
        private readonly TaskCompletionSource<object?> m_TaskCompletionSource = new();

        public AdditiveSceneTask(AssetReferenceComponent<SceneAttribute> sceneAttr, CancellationToken cancellationToken)
        {
            Start(sceneAttr, cancellationToken);
        }

        public override double Progress => m_Progress;

        public override Task Task => m_TaskCompletionSource.Task;

        void AddressablesTaskExtensions.IProgressCallback.OnProgress(double progress)
        {
            m_Progress = progress * kProgressWeightForInstantiate;
        }

        private async void Start(AssetReferenceComponent<SceneAttribute> sceneAttr, CancellationToken cancellationToken)
        {
            SceneAttribute? component = null;

            try
            {
                component = await sceneAttr.InstantiateBoundGameObjectAsync<SceneAttribute>(this, cancellationToken);
                if (!component.Scene.RuntimeKeyIsValid())
                {
                    throw new InvalidOperationException("The scene reference in the SceneAttribute component is not valid.");
                }

                var loadSceneOperationHandle = component.Scene.LoadSceneAsync(LoadSceneMode.Additive);
                try
                {
                    while (!loadSceneOperationHandle.IsDone)
                    {
                        m_Progress = kProgressWeightForInstantiate + loadSceneOperationHandle.PercentComplete * (1 - kProgressWeightForInstantiate - kProgressWeightForActivation);
                        await Task.WhenAny(Task.Delay(TimeSpan.FromSeconds(0.1)), loadSceneOperationHandle.Task);
                    }

                    var asyncOp = loadSceneOperationHandle.Result.ActivateAsync();
                    var waitTask = asyncOp.WaitAsync(cancellationToken);
                    double @base = 1.0 - kProgressWeightForActivation;
                    while (!asyncOp.isDone)
                    {
                        m_Progress = @base + asyncOp.progress * kProgressWeightForActivation;
                        await Task.WhenAny(Task.Delay(TimeSpan.FromSeconds(0.1)), waitTask);
                    }

                    m_Progress = 1.0;
                    m_TaskCompletionSource.SetResult(null);
                    component.m_SceneOperationHandle = loadSceneOperationHandle;
                }
                catch
                {
                    loadSceneOperationHandle.ReleaseHandleOnCompletion();
                    throw;
                }
            }
            catch (OperationCanceledException)
            {
                if (component)
                {
                    Object.Destroy(component.gameObject);
                }

                m_TaskCompletionSource.SetCanceled();
            }
            catch (Exception e)
            {
                if (component)
                {
                    Object.Destroy(component.gameObject);
                }

                m_TaskCompletionSource.SetException(e);
            }
        }
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
        try
        {
            foreach (var task in m_Tasks)
            {
                task.Dispose();
            }

            m_Tasks.Clear();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }

    public void AddTask(LoadTask task)
    {
        m_Tasks.Add(task);
    }

    public void AddAdditive(AssetReferenceComponent<SceneAttribute> sceneAttr)
    {
        m_Tasks.Add(new AdditiveSceneTask(sceneAttr, m_CancellationToken));
    }

    internal Task WhenAll() => Task.WhenAll(m_Tasks.Select(t => t.Task));
}
