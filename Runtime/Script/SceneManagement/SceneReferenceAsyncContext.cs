#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Task2 = System.Threading.Tasks.Task;

namespace Ayla
{
    public partial class SceneReferenceAsyncContext
    {
        public Task<Scene> Task { get; }

        public Scene Result => Task.Result;

        public double Progress { get; private set; }

        public bool IsDone => Progress >= 1.0;

        internal SceneReferenceAsyncContext(AsyncOperation asyncOp, string sceneName, CancellationToken cancellationToken)
        {
            Task = Start();

            return;

            async Task<Scene> Start()
            {
                var task = TaskUtility.Create(async () => await asyncOp);
                try
                {
                    while (asyncOp.isDone == false)
                    {
                        Progress = (double)asyncOp.progress;
                        await Task2.WhenAny(task, Task2.Delay(TimeSpan.FromSeconds(0.1), cancellationToken));
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    Progress = 1;
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
                var sceneCount = SceneManager.sceneCount;
                Scene? lastScene = null;
                for (int i = 0; i < sceneCount; ++i)
                {
                    var sceneAt = SceneManager.GetSceneAt(i);
                    if (sceneAt.name == sceneName)
                    {
                        lastScene = sceneAt;
                    }
                }

                Debug.Assert(lastScene.HasValue, "The scene was loaded but cannot be found in the loaded scenes. This may indicate an issue with the scene loading process.");
                return lastScene!.Value;
            }
        }
    }
}
