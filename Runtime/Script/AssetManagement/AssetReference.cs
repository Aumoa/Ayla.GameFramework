#nullable enable

using System;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ayla
{
    public abstract class AssetReference
    {
        public abstract Type Type { get; }

        public abstract bool IsValid { get; }

        public abstract AssetReferenceType ReferenceType { get; }

        public abstract AssetReferenceAsyncContext LoadGenericAssetAsync(CancellationToken cancellationToken = default);

        public abstract AssetReferenceAsyncContext InstantiateGenericAsync(Transform? parent, CancellationToken cancellationToken = default);

        public AssetReferenceAsyncContext InstantiateGenericAssetAsync(CancellationToken cancellationToken = default)
            => InstantiateGenericAsync(null, cancellationToken);
    }

    [Serializable]
    public partial class AssetReference<T> : AssetReference where T : Object
    {
        [SerializeField]
        private T? m_Asset;

        public override Type Type => typeof(T);

        public override bool IsValid
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

        public override AssetReferenceType ReferenceType
        {
            get
            {
#if WITH_ADDRESSABLES
                if (!string.IsNullOrWhiteSpace(m_AssetGUID))
                {
                    return AssetReferenceType.SoftReference;
                }
#endif

                return m_Asset ? AssetReferenceType.Reference : AssetReferenceType.None;
            }
        }

#if UNITY_EDITOR
        public T? EditorAsset
        {
            get
            {
#if WITH_ADDRESSABLES
                if (!string.IsNullOrWhiteSpace(m_AssetGUID))
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(m_AssetGUID);
                    return AssetDatabase.LoadAssetAtPath<T>(assetPath);
                }
#endif

                return m_Asset;
            }
        }
#endif

        public override AssetReferenceAsyncContext LoadGenericAssetAsync(CancellationToken cancellationToken = default)
            => LoadAssetAsync(cancellationToken);

        public AssetReferenceAsyncContext<T> LoadAssetAsync(CancellationToken cancellationToken = default)
        {
#if WITH_ADDRESSABLES
            if (TryLoadAssetByAddressables(cancellationToken, out var context))
            {
                return context;
            }
#endif

            if (m_Asset)
            {
                return new AssetReferenceAsyncContext<T>(m_Asset);
            }

            throw new InvalidOperationException("The asset reference does not contain a valid asset. Please check IsValid before attempting to load the asset.");
        }

        public override AssetReferenceAsyncContext InstantiateGenericAsync(Transform? parent, CancellationToken cancellationToken = default)
            => InstantiateAsync(parent, cancellationToken);

        public AssetReferenceAsyncContext<T> InstantiateAsync(Transform? parent, CancellationToken cancellationToken = default)
        {
            if (typeof(T) != typeof(GameObject) && typeof(T).IsAssignableTo(typeof(Component)) == false)
            {
                throw new InvalidOperationException("Only GameObject or Component types can be instantiated. Please check the type of the asset reference.");
            }

#if WITH_ADDRESSABLES
            {
                if (TryInstantiateByAddressables(parent, cancellationToken, out var context))
                {
                    return context;
                }
            }
#endif

            if (m_Asset)
            {
                var asyncOp = Object.InstantiateAsync(m_Asset, 1, parent);
                var context = new AssetReferenceAsyncContext<T>(asyncOp, cancellationToken);
                return context;
            }

            throw new InvalidOperationException("The asset reference does not contain a valid asset. Please check IsValid before attempting to instantiate the asset.");
        }

        public AssetReferenceAsyncContext<T> InstantiateAsync(CancellationToken cancellationToken = default)
            => InstantiateAsync(null, cancellationToken);
    }
}
