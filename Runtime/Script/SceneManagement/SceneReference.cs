#nullable enable

using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Ayla
{
    [Serializable]
    public partial class SceneReference
    {
        [SerializeField]
        private Object? m_Asset;

        public bool IsValid
        {
            get
            {
                return
#if WITH_ADDRESSABLES
                    !string.IsNullOrWhiteSpace(m_AssetGUID) ||
#endif
                    m_Asset != null;
            }
        }

        public SceneReferenceAsyncContext LoadSceneAsync(LoadSceneParameters parameters, bool activateOnLoad, int priority, CancellationToken cancellationToken = default)
        {
#if WITH_ADDRESSABLES
            if (TryLoadSceneByAddressables(parameters, activateOnLoad, priority, cancellationToken, out var context))
            {
                return context;
            }
#endif

            if (m_Asset)
            {
                var sceneName = m_Asset.name;
                var asyncOp = SceneManager.LoadSceneAsync(sceneName, parameters);
                return new SceneReferenceAsyncContext(asyncOp, sceneName, cancellationToken);
            }

            throw new InvalidOperationException("The scene reference does not contain a valid scene asset. Please check IsValid before attempting to load the scene.");
        }

        public SceneReferenceAsyncContext LoadSceneAsync(LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100, CancellationToken cancellationToken = default)
            => LoadSceneAsync(new LoadSceneParameters(loadSceneMode), activateOnLoad, priority, cancellationToken);
    }
}
