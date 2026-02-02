using System.Threading;
using System.Threading.Tasks;

namespace Ayla;

public abstract class Singleton
{
    public abstract void Initialize();
    public abstract ValueTask OnEvent(int eventId, CancellationToken cancellationToken);

    public virtual void OnGUI()
    {
    }
}
