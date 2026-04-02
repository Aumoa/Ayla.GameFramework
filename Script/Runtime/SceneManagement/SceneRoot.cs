using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ayla
{
    public class SceneRoot : MonoBehaviour
    {
        private bool m_OnEnableCalled;
        private bool m_OnDisableCalled;

        protected virtual void OnEnable()
        {
            SceneRootManager.Instance.RegisterSceneRoot(this);
            m_OnEnableCalled = true;
        }

        protected virtual void OnDisable()
        {
            if (SceneRootManager.TryGetInstance(out var instance))
            {
                instance.UnregisterSceneRoot(this);
            }

            m_OnDisableCalled = true;
        }

        internal void CheckOnEnableCalled()
        {
            Debug.Assert(m_OnEnableCalled);
        }

        internal void CheckOnDisableCalled()
        {
            Debug.Assert(m_OnDisableCalled);
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
