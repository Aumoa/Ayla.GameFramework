using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Ayla;

public interface IAssetReferenceStorage
{
    void SetAsyncOperationHandle(AsyncOperationHandle<GameObject> op);
}
