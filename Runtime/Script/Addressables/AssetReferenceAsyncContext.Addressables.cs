#nullable enable

#if WITH_ADDRESSABLES

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Ayla
{
    public partial class AssetReferenceAsyncContext<T>
    {
        internal AssetReferenceAsyncContext(AsyncOperationHandle<T> asyncOp, CancellationToken cancellationToken = default)
        {
            Task = Start();

            return;

            async Task<T> Start()
            {
                try
                {
                    var task = asyncOp.Task;
                    m_ProgressGetter = () => asyncOp.PercentComplete;
                    var result = await task.WaitAsync(cancellationToken);
                    ReleaseAction += asyncOp.Release;
                    return result;
                }
                catch
                {
                    asyncOp.ReleaseHandleOnCompletion();
                    throw;
                }
            }
        }

        internal AssetReferenceAsyncContext(AsyncOperationHandle<GameObject> asyncOp, bool instantiate, CancellationToken cancellationToken = default)
        {
            Task = Start();

            return;

            async Task<T> Start()
            {
                var task = asyncOp.Task;
                try
                {
                    m_ProgressGetter = () => asyncOp.PercentComplete;
                    var result = await task.WaitAsync(cancellationToken);
                    if (typeof(T) == typeof(GameObject))
                    {
                        ReleaseAction += asyncOp.Release;
                        return (T)(object)result;
                    }
                    else
                    {
#pragma warning disable UNT0014
                        if (result.TryGetComponent<T>(out var targetComponent))
#pragma warning restore UNT0014
                        {
                            ReleaseAction += asyncOp.Release;
                            return targetComponent;
                        }
                        else
                        {
                            throw new InvalidOperationException($"The instantiated GameObject does not contain the expected component of type {typeof(T).FullName}");
                        }
                    }
                }
                catch
                {
                    if (instantiate)
                    {
                        _ = task.ContinueWith(t =>
                        {
                            if (t.IsCompletedSuccessfully)
                            {
                                var result = t.Result;
                                if (result)
                                {
                                    Object.Destroy(result);
                                }
                            }
                        });
                    }

                    asyncOp.ReleaseHandleOnCompletion();
                    throw;
                }
            }
        }
    }
}

#endif