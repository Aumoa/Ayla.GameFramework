#nullable enable

#if WITH_ADDRESSABLES

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Ayla
{
    public partial class AssetReference<T>
    {
        [SerializeField]
        private string? m_AssetGUID;

        private bool TryLoadAssetByAddressables(CancellationToken cancellationToken, [NotNullWhen(true)] out AssetReferenceAsyncContext<T>? context)
        {
            if (string.IsNullOrWhiteSpace(m_AssetGUID))
            {
                context = null;
                return false;
            }

            if (typeof(T).IsAssignableTo(typeof(Component)))
            {
                var asyncOp = Addressables.LoadAssetAsync<GameObject>(m_AssetGUID);
                context = new AssetReferenceAsyncContext<T>(asyncOp, false, cancellationToken);
            }
            else
            {
                var asyncOp = Addressables.LoadAssetAsync<T>(m_AssetGUID);
                context = new AssetReferenceAsyncContext<T>(asyncOp, cancellationToken);
            }

            return true;
        }

        private bool TryInstantiateByAddressables(Transform? parent, CancellationToken cancellationToken, [NotNullWhen(true)] out AssetReferenceAsyncContext<T>? context)
        {
            if (string.IsNullOrWhiteSpace(m_AssetGUID))
            {
                context = null;
                return false;
            }

            var asyncOp = Addressables.InstantiateAsync(m_AssetGUID, parent);
            context = new AssetReferenceAsyncContext<T>(asyncOp, true, cancellationToken);
            return true;
        }
    }
}

#endif
