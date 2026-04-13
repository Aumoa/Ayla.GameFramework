using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ayla
{
    public class SceneRoot : MonoBehaviour, IAssetReferenceStorage
    {
        private bool m_AwakeCalled;
        private bool m_OnDestroyCalled;

        private readonly List<AssetReferenceAsyncContext> m_AssetOperationHandle = new();

        public void AddAssetReference(AssetReferenceAsyncContext op)
        {
            Debug.Assert(didAwake, "Asset reference must be set after Awake. Please ensure that the asset reference is set during or after the Awake phase.");
            m_AssetOperationHandle.Add(op);
        }

        protected virtual void Awake()
        {
            SceneRootManager.Instance.RegisterSceneRoot(this);
            m_AwakeCalled = true;
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnDestroy()
        {
            foreach (var aop in m_AssetOperationHandle)
            {
                aop.Release();
            }

            if (SceneRootManager.TryGetInstance(out var instance))
            {
                instance.UnregisterSceneRoot(this);
            }

            m_AssetOperationHandle.Clear();
            m_OnDestroyCalled = true;
        }

        internal void CheckAwakeCalled()
        {
            Debug.Assert(m_AwakeCalled);
        }

        internal void CheckOnDestroyCalled()
        {
            Debug.Assert(m_OnDestroyCalled);
        }

        public virtual ValueTask BeforeUnloadSceneAsync(CancellationToken cancellationToken = default)
        {
            return default;
        }

        public virtual ValueTask UnloadingSceneAsync(SceneUnloadingContext context, CancellationToken cancellationToken = default)
        {
            context.AddUnloadAllScenes();
            return default;
        }

        public virtual ValueTask LoadingSceneAsync(SceneLoadingContext context, CancellationToken cancellationToken = default)
        {
            return default;
        }

        public virtual ValueTask AfterLoadSceneAsync(CancellationToken cancellationToken = default)
        {
            return default;
        }
    }
}
