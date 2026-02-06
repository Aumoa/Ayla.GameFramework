using System.Threading;
using System.Threading.Tasks;

namespace Ayla;

public abstract class Singleton
{
    internal static class ConstructorContext
    {
        public static ThreadLocal<SingletonManager?> Manager = new();

        public static void Begin(SingletonManager manager)
        {
            Manager.Value = manager;
        }

        public static void End()
        {
            Manager.Value = null;
        }
    }

    public SingletonManager Manager { get; }

    public Singleton()
    {
#pragma warning disable UNT0007
        Manager = ConstructorContext.Manager.Value
            ?? throw new System.InvalidOperationException("Singletons can only be constructed during SingletonManager initialization.");
#pragma warning restore UNT0007
    }

    public virtual ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        return default;
    }

    public virtual ValueTask PostInitializeAsync(CancellationToken cancellationToken = default)
    {
        return default;
    }

    public virtual ValueTask OnEvent(int eventId, CancellationToken cancellationToken = default)
    {
        return default;
    }

    public virtual void Update()
    {
    }

    public virtual void LateUpdate()
    {
    }

    public virtual void FixedUpdate()
    {
    }

    public virtual void OnGUI()
    {
    }
}
