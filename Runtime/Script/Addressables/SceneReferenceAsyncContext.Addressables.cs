#nullable enable

#if WITH_ADDRESSABLES

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Task2 = System.Threading.Tasks.Task;

namespace Ayla
{
    public partial class SceneReferenceAsyncContext
    {
        internal SceneReferenceAsyncContext(AsyncOperationHandle<SceneInstance> asyncOp, CancellationToken cancellationToken)
        {
            Task = Start();

            return;

            async Task<Scene> Start()
            {
                var task = asyncOp.Task;
                try
                {
                    while (asyncOp.IsDone == false)
                    {
                        Progress = (double)asyncOp.PercentComplete;
                        await Task2.WhenAny(task, Task2.Delay(TimeSpan.FromSeconds(0.1), cancellationToken));
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    Progress = 1;
                    return task.Result.Scene;
                }
                catch
                {
                    asyncOp.ReleaseHandleOnCompletion();
                    throw;
                }
            }
        }
    }
}

#endif
