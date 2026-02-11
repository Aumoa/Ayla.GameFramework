using UnityEngine;

namespace Ayla;

public abstract class SingletonBehaviour<TData> : MonoBehaviour where TData : SingletonData
{
    [SerializeField]
    private TData m_Data;

    public ref TData Data => ref m_Data;
}
