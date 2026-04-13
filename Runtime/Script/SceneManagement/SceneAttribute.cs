#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ayla
{
    public class SceneAttribute : MonoBehaviour, IAssetReferenceStorage
    {
        public SceneAttributeType Type;
        public SceneReference Scene;

        private readonly List<AssetReferenceAsyncContext> m_AssetOperationHandle = new();
        internal Scene m_SceneLoaded;

        public void AddAssetReference(AssetReferenceAsyncContext op)
        {
            Debug.Assert(didAwake, "Asset reference must be set after Awake. Please ensure that the asset reference is set during or after the Awake phase.");
            m_AssetOperationHandle.Add(op);
        }

        protected virtual void OnDestroy()
        {
            foreach (var aop in m_AssetOperationHandle)
            {
                aop.Release();
            }

            m_AssetOperationHandle.Clear();
        }

        public Scene GetScene() => m_SceneLoaded;
    }
}
