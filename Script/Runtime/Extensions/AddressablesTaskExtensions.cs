using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Ayla;

public static class AddressablesTaskExtensions
{
    public interface IProgressCallback
    {
        double Interval => 0.1;

        double LoadAssetProgressWeight => 0.5;

        void OnProgress(double progress);
    }

    public static async ValueTask<TComponent> InstantiateBoundGameObjectAsync<TComponent>(this AssetReferenceGameObject reference, IProgressCallback? callback, CancellationToken cancellationToken = default)
        where TComponent : Component, IAssetReferenceStorage
    {
        var loadAssetOp = reference.InstantiateAsync();
        try
        {
            GameObject prefab;
            {
                var waitAsyncTask = loadAssetOp.Task.WaitAsync(cancellationToken);
                if (callback == null)
                {
                    prefab = await waitAsyncTask;
                }
                else
                {
                    while (!loadAssetOp.IsDone)
                    {
                        callback.OnProgress(loadAssetOp.PercentComplete * callback.LoadAssetProgressWeight);
                        // Awaiting Task.WhenAny ensures that if waitAsyncTask is cancelled and throws an OperationCanceledException, the appropriate exception handler is invoked immediately to handle the interruption.
                        await await Task.WhenAny(Task.Delay(TimeSpan.FromSeconds(callback.Interval)), waitAsyncTask);
                    }

                    prefab = loadAssetOp.Result;
                }
            }

            if (prefab == null)
            {
                throw new InvalidOperationException("Failed to load asset.");
            }

            Debug.Assert(prefab.TryGetComponent<TComponent>(out _));

            var instantiateOp = Object.InstantiateAsync(prefab);
            try
            {
                GameObject gameObject;
                var waitAsyncTask = instantiateOp.WaitAsync(cancellationToken).AsTask();
                if (callback == null)
                {
                    gameObject = (await waitAsyncTask)[0];
                }
                else
                {
                    while (!instantiateOp.isDone)
                    {
                        callback.OnProgress(callback.LoadAssetProgressWeight + instantiateOp.progress * (1.0 - callback.LoadAssetProgressWeight));
                        await await Task.WhenAny(Task.Delay(TimeSpan.FromSeconds(callback.Interval)), waitAsyncTask);
                    }

                    gameObject = instantiateOp.Result[0];
                }

                if (gameObject == null)
                {
                    throw new InvalidOperationException("Failed to instantiate asset.");
                }

                if (!gameObject.TryGetComponent(out TComponent component))
                {
                    throw new ArgumentException("The instantiated GameObject does not contain the required component.");
                }

                Debug.Assert(gameObject.activeInHierarchy, "GameObject must be active in hierarchy; otherwise, OnDestroy() will not be called, which may prevent AsyncOperationHandle from being released properly.");
                component.SetAsyncOperationHandle(loadAssetOp);

                return component;
            }
            catch
            {
                Debug.Assert(ApplicationMisc.IsInMainThread(), "Cancellation should occur on the main thread to ensure that Unity objects are manipulated safely.");
                if (!instantiateOp.isDone)
                {
                    instantiateOp.Cancel();
                }

                instantiateOp.completed += _ =>
                {
                    foreach (var gameObject in instantiateOp.Result)
                    {
                        if (gameObject)
                        {
                            Object.Destroy(gameObject);
                        }
                    }
                };

                throw;
            }
        }
        catch
        {
            loadAssetOp.ReleaseHandleOnCompletion();
            throw;
        }
    }
}
