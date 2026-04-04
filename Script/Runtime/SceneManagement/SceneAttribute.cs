using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Ayla
{
    public class SceneAttribute : MonoBehaviour, IAssetReferenceStorage
    {
        public SceneAttributeType Type;
        public SceneReference Scene;

        private AsyncOperationHandle<GameObject> m_AssetOperationHandle;
        internal AsyncOperationHandle<SceneInstance> m_SceneOperationHandle;

        void IAssetReferenceStorage.SetAsyncOperationHandle(AsyncOperationHandle<GameObject> op)
        {
            Debug.Assert(m_AssetOperationHandle.IsValid() == false);
            Debug.Assert(didAwake);
            m_AssetOperationHandle = op;
        }

        protected virtual void OnDestroy()
        {
            if (m_AssetOperationHandle.IsValid())
            {
                m_AssetOperationHandle.Release();
            }

            if (m_SceneOperationHandle.IsValid())
            {
                m_SceneOperationHandle.Release();
            }
        }
    }
}
