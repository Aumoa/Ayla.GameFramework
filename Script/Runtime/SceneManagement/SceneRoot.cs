using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Ayla
{
    public class SceneRoot : MonoBehaviour
    {
        private bool m_AwakeCalled;
        private bool m_OnDestroyCalled;

        internal AsyncOperationHandle<GameObject> m_AssetOperationHandle;

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
            if (SceneRootManager.TryGetInstance(out var instance))
            {
                instance.UnregisterSceneRoot(this);
            }

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
