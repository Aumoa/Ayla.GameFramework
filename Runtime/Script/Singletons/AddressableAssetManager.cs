#nullable enable

#if WITH_ADDRESSABLES

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Ayla
{
    public class AddressableAssetManager : Singleton
    {
        public override async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
        {
            await Addressables.InitializeAsync().Task;
            Debug.LogFormat("Addressables initialized.");
        }
    }
}

#endif
