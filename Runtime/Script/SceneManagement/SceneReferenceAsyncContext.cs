#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ayla
{
    public partial class SceneReferenceAsyncContext
    {
        public Task<Scene> Task { get; }

        public Scene Result => Task.Result;

        public double Progress
        {
            get
            {
                if (Task.IsCompleted)
                {
                    return 1.0;
                }

                return m_ProgressGetter?.Invoke() ?? 0;
            }
        }

        public bool IsDone => Task.IsCompleted;

        private Func<double>? m_ProgressGetter;

        internal SceneReferenceAsyncContext(AsyncOperation asyncOp, int buildIndex, CancellationToken cancellationToken)
        {
            Task = Start();

            return;

            async Task<Scene> Start()
            {
                var task = TaskUtility.Create(async () => await asyncOp).AsTask();
                try
                {
                    m_ProgressGetter = () => asyncOp.progress;
                    await task.WaitAsync(cancellationToken);
                    return GetLoadedScene();
                }
                catch
                {
                    _ = task.ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            var scene = GetLoadedScene();
                            _ = SceneManager.UnloadSceneAsync(scene);
                        }
                    });

                    throw;
                }
            }

            Scene GetLoadedScene()
            {
                var sceneAt = SceneManager.GetSceneByBuildIndex(buildIndex);
                Debug.Assert(sceneAt.buildIndex != -1, "The scene was loaded but cannot be found in the loaded scenes. This may indicate an issue with the scene loading process.");
                return sceneAt;
            }
        }
    }
}
