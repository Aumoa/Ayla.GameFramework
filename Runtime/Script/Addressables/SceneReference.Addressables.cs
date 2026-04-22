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
        private readonly string? m_Address;

        private SceneReference(string address)
        {
            m_Address = address;
        }

        private bool TryLoadSceneByAddressables(LoadSceneParameters parameters, bool activateOnLoad, int priority, CancellationToken cancellationToken, [NotNullWhen(true)] out SceneReferenceAsyncContext? context)
        {
            string runtimeKey;
            if (string.IsNullOrWhiteSpace(m_Address) == false)
            {
                runtimeKey = m_Address;
            }
            else if (string.IsNullOrWhiteSpace(m_AssetGUID) == false)
            {
                runtimeKey = m_AssetGUID;
            }
            else
            {
                context = null;
                return false;
            }

            var asyncOp = Addressables.LoadSceneAsync(runtimeKey, parameters, SceneReleaseMode.ReleaseSceneWhenSceneUnloaded, activateOnLoad, priority);
            context = new SceneReferenceAsyncContext(asyncOp, cancellationToken);
            return true;
        }

        public static SceneReference FromAddress(string address)
        {
            return new SceneReference(address);
        }

#if UNITY_EDITOR
        public static SceneReference FromAddressablesAsset(string guid)
        {
            return new SceneReference
            {
                m_AssetGUID = guid
            };
        }
#endif
    }
}
#endif
