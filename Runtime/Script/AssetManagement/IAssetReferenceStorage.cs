#nullable enable

namespace Ayla
{
    public interface IAssetReferenceStorage
    {
        void AddAssetReference(AssetReferenceAsyncContext op);
    }
}
