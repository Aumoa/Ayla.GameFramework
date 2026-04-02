using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Ayla;

public class SceneRootManager : Singleton<SceneRootManager, SceneRootManagerData>
{
    private readonly List<SceneRoot> m_SceneRoots = new();

    private SemaphoreSlim m_Semaphore;

    protected override void Awake()
    {
        base.Awake();
        m_Semaphore = new SemaphoreSlim(1);
    }

    protected override void OnDestroy()
    {
        m_Semaphore?.Dispose();
        base.OnDestroy();
    }

    public override ValueTask PostInitializeAsync(CancellationToken cancellationToken = default)
    {
        if (Data.InitialScene.RuntimeKeyIsValid() == false)
        {
            Debug.LogErrorFormat("Initial scene asset reference is not set or invalid. Please set a valid scene asset reference in the SceneRootManagerData.");
            return default;
        }

        LoadSceneAsync(Data.InitialScene).Forget();
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

    public async ValueTask LoadSceneAsync(AssetReferenceGameObject sceneRootAsset, CancellationToken cancellationToken = default)
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
            }
            catch (OperationCanceledException)
            {
                _ = loadAssetOp.Task.ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully && t.Result)
                    {
                        Destroy(t.Result.gameObject);
                    }
                });

                throw;
            }
            finally
            {
                beforeUnloadSceneTask?.Forget();
            }

            if (sceneRootPrefab == null)
            {
                throw new InvalidOperationException($"Failed to load scene root from asset {sceneRootAsset}");
            }

            // Create a new empty scene and set it as active to ensure that the instantiated scene root will be in the new scene.
            Scene emptyScene = SceneManager.CreateScene(sceneRootPrefab.name);
            SceneManager.SetActiveScene(emptyScene);
            SceneRoot sceneRoot = InactiveObject.Instantiate(sceneRootPrefab);

            using (var unloadingContext = new SceneUnloadingContext(emptyScene, cancellationToken))
            {
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
                    previousSceneRoot!.CheckOnDisableCalled();
                }
            }

            // Set the new scene root active after the old scene is completely unloaded to avoid potential issues caused by having multiple active scene roots.
            sceneRoot.gameObject.SetActive(true);

            using (var loadingContext = new SceneLoadingContext(cancellationToken))
            {
                sceneRoot.CheckOnEnableCalled();
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
