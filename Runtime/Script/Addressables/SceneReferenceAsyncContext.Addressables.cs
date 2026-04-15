#nullable enable

#if WITH_ADDRESSABLES

using System.Threading;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

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
                    m_ProgressGetter = () => asyncOp.PercentComplete;
                    var sceneInstance = await task.WaitAsync(cancellationToken);
                    return sceneInstance.Scene;
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
