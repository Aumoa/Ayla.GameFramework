using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ayla;

public class SceneLoadManager : Singleton<SceneLoadManagerData>
{
    public override async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (Data.StartupScene.RuntimeKeyIsValid() == false)
        {
            throw new InvalidOperationException();
        }
    }
}
