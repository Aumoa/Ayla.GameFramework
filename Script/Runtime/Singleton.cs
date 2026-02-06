using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ayla;

public abstract class Singleton : MonoBehaviour
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

    [SerializeField]
    private SingletonManager? m_Manager;

    public SingletonManager Manager
    {
        get
        {
            if (m_Manager == null)
            {
                throw new System.InvalidOperationException("Singletons can only be constructed during SingletonManager initialization.");
            }

            return m_Manager;
        }
    }

    public Singleton()
    {
        m_Manager = ConstructorContext.Manager.Value;
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
}
