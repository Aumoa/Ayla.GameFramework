#nullable enable

#if WITH_ADDRESSABLES

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Ayla
{
    public partial class SceneReference
    {
        [SerializeField]
        private string? m_AssetGUID;

        private bool TryLoadSceneByAddressables(LoadSceneParameters parameters, bool activateOnLoad, int priority, CancellationToken cancellationToken, [NotNullWhen(true)] out SceneReferenceAsyncContext? context)
        {
            if (string.IsNullOrWhiteSpace(m_AssetGUID))
            {
                context = null;
                return false;
            }

            var asyncOp = Addressables.LoadSceneAsync(m_AssetGUID, parameters, SceneReleaseMode.ReleaseSceneWhenSceneUnloaded, activateOnLoad, priority);
            context = new SceneReferenceAsyncContext(asyncOp, cancellationToken);
            return true;
        }
    }
}
#endif
