#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Ayla
{
    public class SceneRootManager : Singleton<SceneRootManager, SceneRootManagerData>
    {
        [SerializeField, HideInInspector]
        private List<SceneRoot> m_SceneRoots = new();

        private SemaphoreSlim m_Semaphore = null!;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Semaphore ??= new SemaphoreSlim(1);
        }

        protected override void OnDestroy()
        {
            m_Semaphore?.Dispose();
            base.OnDestroy();
        }

        public override ValueTask StartAsync(CancellationToken cancellationToken = default)
        {
            if (Data.InitialScene.IsValid == false)
            {
                Debug.LogErrorFormat("Initial scene asset reference is not set or invalid. Please set a valid scene asset reference in the SceneRootManagerData.");
                return default;
            }

            AssetReference<SceneRoot> initialScene = Data.InitialScene;

#if UNITY_EDITOR
            if (Data.EditorOverrideScene.IsValid)
            {
                initialScene = Data.EditorOverrideScene;
            }
#endif

            LoadSceneAsync(initialScene).Forget();
            return default;
        }

        public void RegisterSceneRoot(SceneRoot sceneRoot)
        {
            m_SceneRoots.Add(sceneRoot);
        }

        public void UnregisterSceneRoot(SceneRoot sceneRoot)
        {
            m_SceneRoots.Remove(sceneRoot);
        }

        public async ValueTask LoadSceneAsync(AssetReference<SceneRoot> sceneRootAsset, CancellationToken cancellationToken = default)
        {
            await m_Semaphore.WaitAsync(cancellationToken);

            try
            {
                var previousSceneRoot = GetCurrentSceneRoot();
                var loadAssetOp = sceneRootAsset.LoadAssetAsync();
                SceneRoot sceneRootPrefab;
                ValueTask? beforeUnloadSceneTask = previousSceneRoot ? previousSceneRoot.BeforeUnloadSceneAsync(cancellationToken) : null;
                try
                {
                    var prefab = await loadAssetOp.Task.WaitAsync(cancellationToken);
                    if (!prefab.TryGetComponent(out sceneRootPrefab))
                    {
                        throw new InvalidOperationException("The loaded scene root prefab does not contain a SceneRoot component.");
                    }

                    if (beforeUnloadSceneTask.HasValue)
                    {
                        await beforeUnloadSceneTask.Value;
                    }
                }
                catch (OperationCanceledException)
                {
                    loadAssetOp.ReleaseHandleOnCompletion();
                    beforeUnloadSceneTask?.Forget();
                    throw;
                }

                if (sceneRootPrefab == null)
                {
                    throw new InvalidOperationException($"Failed to load scene root from asset {sceneRootAsset}");
                }

                Scene emptyScene;
                SceneRoot sceneRoot;

                try
                {
                    // Create a new empty scene and set it as active to ensure that the instantiated scene root will be in the new scene.
                    emptyScene = SceneManager.CreateScene(sceneRootPrefab.name);
                    SceneManager.SetActiveScene(emptyScene);
                    sceneRoot = InactiveObject.Instantiate(sceneRootPrefab);

                    using var unloadingContext = new SceneUnloadingContext(emptyScene, cancellationToken);
                    bool hasPreviousScene = previousSceneRoot != null;
                    if (hasPreviousScene)
                    {
                        await previousSceneRoot!.UnloadingSceneAsync(unloadingContext, cancellationToken);
                    }
                    else
                    {
                        unloadingContext.AddUnloadAllScenes();
                    }

                    await unloadingContext.WhenAll().WaitAsync(cancellationToken);

                    if (hasPreviousScene)
                    {
                        // Object destroyal and scene unloading will cause OnDisable to be called on the previous scene root, so we check if OnDisable has been called to ensure that all related cleanup has been done before proceeding.
                        previousSceneRoot!.CheckOnDestroyCalled();
                    }
                }
                catch
                {
                    loadAssetOp.Release();
                    throw;
                }

                // Set the new scene root active after the old scene is completely unloaded to avoid potential issues caused by having multiple active scene roots.
                sceneRoot.gameObject.SetActive(true);
                sceneRoot.AddAssetReference(loadAssetOp);
                sceneRoot.CheckAwakeCalled();

                using (var loadingContext = new SceneLoadingContext(cancellationToken))
                {
                    sceneRoot.CheckAwakeCalled();
                    await sceneRoot.LoadingSceneAsync(loadingContext, cancellationToken);
                    await loadingContext.WhenAll().WaitAsync(cancellationToken);
                }

                await sceneRoot.AfterLoadSceneAsync(cancellationToken);
            }
            finally
            {
                m_Semaphore.Release();
            }
        }

        public SceneRoot? GetCurrentSceneRoot()
        {
#if UNITY_EDITOR
            foreach (var sceneRoot in m_SceneRoots)
            {
                Debug.Assert(sceneRoot);
            }
#endif

            return m_SceneRoots.LastOrDefault();
        }
    }
}
